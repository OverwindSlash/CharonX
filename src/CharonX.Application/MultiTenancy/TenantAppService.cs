﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Organizations;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Editions;
using CharonX.MultiTenancy.Dto;
using CharonX.Organizations;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Caching;
using Microsoft.EntityFrameworkCore;

namespace CharonX.MultiTenancy
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class TenantAppService : AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>, ITenantAppService
    {
        public static string TenantAdminCacheName = "TenantAdmin";

        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly RoleManager _roleManager;
        private readonly OrganizationUnitManager _orgUnitManager;
        private readonly ICacheManager _cacheManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;

        public TenantAppService(
            IRepository<Tenant, int> repository,
            TenantManager tenantManager,
            EditionManager editionManager,
            UserManager userManager,
            IRepository<User, long> userRepository,
            RoleManager roleManager,
            OrganizationUnitManager orgUnitManager,
            ICacheManager cacheManager,
            IAbpZeroDbMigrator abpZeroDbMigrator
            )
            : base(repository)
        {
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _orgUnitManager = orgUnitManager;
            _cacheManager = cacheManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
        {
            CheckCreatePermission();

            // Create tenant
            var tenant = ObjectMapper.Map<Tenant>(input);
            //tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
            //    ? null
            //    : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

            var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
            if (defaultEdition != null)
            {
                tenant.EditionId = defaultEdition.Id;
            }

            await CheckDuplicatedPhoneNumber(input.AdminPhoneNumber);
            await CheckDuplicatedEmail(input.AdminEmailAddress);

            await _tenantManager.CreateAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

            // Create tenant database
            //_abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

            // We are working entities of new tenant, so changing tenant filter
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                // Create static roles for new tenant
                CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));

                await CurrentUnitOfWork.SaveChangesAsync(); // To get static role ids

                // Grant all permissions to admin role
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                await _roleManager.GrantAllPermissionsAsync(adminRole);

                // Create admin organization unit and set admin role
                var adminOrgUnit = OrganizationUnitHelper.CreateDefaultAdminOrgUnit(tenant.Id);
                await _orgUnitManager.CreateAsync(adminOrgUnit);
                await CurrentUnitOfWork.SaveChangesAsync(); // To get static organization id
                await _roleManager.SetOrganizationUnitsAsync(adminRole, new[] { adminOrgUnit.Id });

                // Create admin user for the tenant
                var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress, input.AdminPhoneNumber);
                await _userManager.InitializeOptionsAsync(tenant.Id);
                CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword));
                await CurrentUnitOfWork.SaveChangesAsync(); // To get admin user's id

                // Assign admin user to AdminGroup ou.
                await _userManager.AddToOrganizationUnitAsync(adminUser, adminOrgUnit);

                // Assign admin user to role!
                CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var dto = MapToEntityDto(tenant);
            await SetPhoneNumberAndEmailAddress(dto);

            return dto;
        }

        private async Task CheckDuplicatedPhoneNumber(string phoneNumber)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (await _userManager.CheckDuplicateMobilePhoneAsync(phoneNumber))
                {
                    throw new UserFriendlyException(L("PhoneNumberDuplicated", phoneNumber));
                }
            }
        }

        private async Task CheckDuplicatedEmail(string email)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (await _userManager.CheckDuplicateEmailAsync(email))
                {
                    throw new UserFriendlyException(L("EmailAddressDuplicated", email));
                }
            }
        }

        public override async Task<TenantDto> GetAsync(EntityDto<int> input)
        {
            var dto = await base.GetAsync(input);
            await SetPhoneNumberAndEmailAddress(dto);

            return dto;
        }

        public override async Task<PagedResultDto<TenantDto>> GetAllAsync(PagedTenantResultRequestDto input)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var tenants= await base.GetAllAsync(input);
            sw.Stop();
            Console.WriteLine($"TenantAppService.GetAllAsync:{sw.ElapsedMilliseconds}");

            sw.Reset();
            sw.Start();
            foreach (var tenant in tenants.Items)
            {
                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                {
                    try
                    {
                        await SetPhoneNumberAndEmailAddress(tenant);
                    }
                    catch (System.Exception)
                    {
                        throw new UserFriendlyException("Can not find the admin user of tenant " + tenant.Name);
                    }
                }
            }
            sw.Stop();
            Console.WriteLine($"TenantAppService.foreach:{sw.ElapsedMilliseconds}");

            return tenants;
        }

        private async Task SetPhoneNumberAndEmailAddress(TenantDto tenant)
        {
            ICache tenantAdminCache = _cacheManager.GetCache(TenantAdminCacheName);

            string phoneAndEmail = await tenantAdminCache.GetAsync(tenant.Id, () => QueryDefaultAdminInfo(tenant.Id));

            if (string.IsNullOrEmpty(phoneAndEmail))
            {
                return;
            }

            string[] infoArray = phoneAndEmail.Split('|');
            if (infoArray.Length != 2)
            {
                return;
            }
            tenant.AdminPhoneNumber = infoArray[0];
            tenant.AdminEmailAddress = infoArray[1];
        }

        private async Task<string> QueryDefaultAdminInfo(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var adminUser = await _userManager.FindByNameAsync(StaticRoleNames.Tenants.Admin);

                return $"{adminUser.PhoneNumber}|{adminUser.EmailAddress}";
            }
        }

        protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), 
                    x => 
                        x.TenancyName.Contains(input.Keyword) || 
                        x.Name.Contains(input.Keyword) ||
                        x.Contact.Contains(input.Keyword) ||
                        x.AdminPhoneNumber.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        //protected override void MapToEntity(TenantDto updateInput, Tenant entity)
        //{
        //    // Manually mapped since TenantDto contains non-editable properties too.
        //    entity.Name = updateInput.Name;
        //    entity.TenancyName = updateInput.TenancyName;
        //    entity.Contact = updateInput.Contact;
        //    entity.Address = updateInput.Address;
        //    entity.Logo = updateInput.Logo;
        //    entity.LogoNode = updateInput.LogoNode;
        //    entity.IsActive = updateInput.IsActive;
        //}

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var tenant = await _tenantManager.GetByIdAsync(input.Id);

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                var users = _userRepository.GetAll().ToList();
                foreach (User user in users)
                {
                    await _userRepository.DeleteAsync(user);
                }
            }

            await _tenantManager.DeleteAsync(tenant);

            await RemoveTenantAdminCacheItem(tenant.Id);
        }

        private async Task RemoveTenantAdminCacheItem(int tenantId)
        {
            ICache tenantAdminCache = _cacheManager.GetCache(TenantAdminCacheName);
            await tenantAdminCache.RemoveAsync(tenantId.ToString());
        }

        public async Task<bool> ActivateTenant(ActivateTenantDto input)
        {
            var tenant = await Repository.GetAsync(input.TenantId);
            if (tenant == null)
            {
                return false;
            }

            tenant.IsActive = input.IsActive;
            await _tenantManager.UpdateAsync(tenant);

            return true;
        }

        private void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public override async Task<TenantDto> UpdateAsync(TenantDto input)
        {
            var tenant = await base.UpdateAsync(input);
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                try
                {
                    var adminUser = await _userManager.FindByNameAsync(StaticRoleNames.Tenants.Admin);

                    if (adminUser.PhoneNumber != input.AdminPhoneNumber)
                    {
                        await CheckDuplicatedPhoneNumber(input.AdminPhoneNumber);
                        adminUser.PhoneNumber = input.AdminPhoneNumber;
                    }

                    if (adminUser.EmailAddress != input.AdminEmailAddress)
                    {
                        await CheckDuplicatedEmail(input.AdminEmailAddress);
                        adminUser.EmailAddress = input.AdminEmailAddress;
                    }

                    await RemoveTenantAdminCacheItem(tenant.Id);
                }
                catch (UserFriendlyException ex)
                {
                    throw ex;
                }
                catch (System.Exception)
                {
                    throw new UserFriendlyException("Can not find the admin user of tenant " + tenant.Name);
                }

            }
            return tenant;
        }
    }
}

