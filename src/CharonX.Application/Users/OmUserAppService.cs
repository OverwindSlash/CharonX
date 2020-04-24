using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.IdentityFramework;
using Abp.Organizations;
using Abp.UI;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Organizations;
using Microsoft.AspNetCore.Identity;

namespace CharonX.Users
{
    public class OmUserAppService : ApplicationService, IOmUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public OmUserAppService(
            UserManager userManager,
            RoleManager roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public async Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = ObjectMapper.Map<User>(input);

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                await CheckDuplicatedPhoneNumber(user);

                CheckErrors(await _userManager.CreateAsync(user, input.Password));

                // Set organization units and roles belongs to them
                if (input.OrgUnitNames != null)
                {
                    CheckErrors(await _userManager.SetOrgUnitsAsync(user, input.OrgUnitNames));
                    CurrentUnitOfWork.SaveChanges();
                }

                // Add additional roles not included in organization units
                if (input.RoleNames != null)
                {
                    CheckErrors(await _userManager.AddToAdditionalRolesAsync(user, input.RoleNames));
                    CurrentUnitOfWork.SaveChanges();
                }

                return await GetUserInTenantAsync(tenantId, new EntityDto<long>(user.Id));
            }
        }

        private async Task CheckDuplicatedPhoneNumber(User user)
        {
            if (await _userManager.CheckDuplicateMobilePhoneAsync(user.PhoneNumber))
            {
                throw new UserFriendlyException(L("PhoneNumberDuplicated", user.PhoneNumber));
            }
        }

        public async Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            input.OrgUnitNames = input.OrgUnitNames.Append(OrganizationUnitHelper.DefaultAdminOrgUnitName).ToArray();

            return await CreateUserInTenantAsync(tenantId, input);
        }

        public async Task<List<UserDto>> GetAllAdminUserInTenantAsync(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var adminRole = await _roleManager.GetRoleByNameAsync(StaticRoleNames.Tenants.Admin);

                IList<User> adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);

                List<UserDto> userDtos = new List<UserDto>();
                foreach (User adminUser in adminUsers)
                {
                    UserDto userDto = ObjectMapper.Map<UserDto>(adminUser);

                    userDto.OrgUnitNames = await GetOrgUnitsOfUserAsync(adminUser);
                    userDto.RoleNames = await GetRolesOfUserAsync(adminUser);
                    userDto.Permissions = await GetPermissionsOfUserAsync(adminUser);

                    userDtos.Add(userDto);
                }

                return userDtos;
            }
        }

        public async Task<UserDto> GetUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(input.Id);
                    var userDto = ObjectMapper.Map<UserDto>(user);

                    userDto.OrgUnitNames = await GetOrgUnitsOfUserAsync(user);
                    userDto.RoleNames = await GetRolesOfUserAsync(user);
                    userDto.Permissions = await GetPermissionsOfUserAsync(user);

                    return userDto;
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }
            }
        }

        private async Task<string[]> GetOrgUnitsOfUserAsync(User user)
        {
            var orgUnits = await _userManager.GetOrganizationUnitsAsync(user);
            return orgUnits.Select(ou => ou.DisplayName).ToArray();
        }

        private async Task<string[]> GetRolesOfUserAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToArray();
        }

        private async Task<string[]> GetPermissionsOfUserAsync(User user)
        {
            var permissions = await _userManager.GetGrantedPermissionsAsync(user);
            return permissions.Select(p => p.Name).ToArray();
        }

        public Task<UserDto> UpdateUserInTenantAsync(int tenantId, UserDto input)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ActivateUserInTenantAsync(int tenantId, ActivateUserDto input)
        {
            throw new NotImplementedException();
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
