﻿using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.UI;
using CharonX.Authorization.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharonX.Authorization.Users
{
    public class UserManager : AbpUserManager<Role, User>
    {
        private readonly RoleManager _roleManager;
        private readonly UserStore _store;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly IRepository<UserRole, long> _userRoleRepository;
        private readonly ICacheManager _cacheManager;
        public const string TenantContactMethodCacheName = "TenantContactMethod";
        public UserManager(
            RoleManager roleManager,
            UserStore store,
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<User> passwordHasher, 
            IEnumerable<IUserValidator<User>> userValidators, 
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<User>> logger, 
            IPermissionManager permissionManager, 
            IUnitOfWorkManager unitOfWorkManager, 
            ICacheManager cacheManager, 
            IRepository<OrganizationUnit, long> organizationUnitRepository, 
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<UserRole, long> userRoleRepository,
            IOrganizationUnitSettings organizationUnitSettings, 
            ISettingManager settingManager)
            : base(
                roleManager, 
                store, 
                optionsAccessor, 
                passwordHasher, 
                userValidators, 
                passwordValidators, 
                keyNormalizer, 
                errors, 
                services, 
                logger, 
                permissionManager, 
                unitOfWorkManager, 
                cacheManager,
                organizationUnitRepository, 
                userOrganizationUnitRepository, 
                organizationUnitSettings, 
                settingManager)
        {
            _roleManager = roleManager;
            _store = store;
            _orgUnitRepository = organizationUnitRepository;
            _userRoleRepository = userRoleRepository;
            _cacheManager = cacheManager;
            LocalizationSourceName = "CharonX";
        }

        public async Task<IdentityResult> SetOrgUnitsAndRoles(User user, string[] orgUnitNames, string[] roleNames)
        {
            List<Role> rolesToBeGrant = new List<Role>();
            var currentRoles = await GetRolesAsync(user);

            // Set organization units and get roles belongs to them.
            var currentOrgs = await GetOrganizationUnitsAsync(user);
            orgUnitNames = orgUnitNames == null ? new string[0] : orgUnitNames;
            //remove orgs
            if (orgUnitNames.Intersect(currentOrgs.Select(p=>p.DisplayName)).ToArray().Length<currentOrgs.Count)
            {
                foreach (var org in currentOrgs)
                {
                    RemoveFromOrganizationUnit(user, org);
                }
            }

            if ((orgUnitNames != null) && (orgUnitNames.Length != 0))
            {
                var organizationUnits = _orgUnitRepository.GetAll()
                    .Where(ou => orgUnitNames.Contains(ou.DisplayName)).ToList();

                await SetOrganizationUnitsAsync(user, organizationUnits.Select(ou => ou.Id).ToArray());

                foreach (OrganizationUnit organizationUnit in organizationUnits)
                {
                    rolesToBeGrant.AddRange(await _roleManager.GetRolesInOrganizationUnit(organizationUnit));
                }
            }


            // Add additional roles not included in organization units
            if ((roleNames != null) && (roleNames.Length != 0))
            {
                foreach (string roleName in roleNames)
                {
                    var role = await _roleManager.GetRoleByNameAsync(roleName);
                    rolesToBeGrant.Add(role);
                }
            }

            // Remove all exist roles

            //var grantedRoles = await GetRolesAsync(user);
            if (rolesToBeGrant.Select(p=>p.DisplayName).Intersect(currentRoles).ToArray().Length<currentRoles.Count)
            {
                foreach (string grantedRole in currentRoles.Except(rolesToBeGrant.Select(p => p.DisplayName)))
                {
                    if (await RemoveFromRoleAsync(user, grantedRole) != IdentityResult.Success)
                    {
                        return IdentityResult.Failed();
                    }
                }
            }
            
            // Add new roles
            return await AddToAdditionalRolesAsync(user, rolesToBeGrant.Distinct().Select(r => r.Name).ToArray());
        }

        public async Task<IdentityResult> AddToAdditionalRolesAsync(User user, string[] roleNames)
        {
            var alreadyInRoles = await GetRolesAsync(user);

            var roles = roleNames.Where(roleName => !alreadyInRoles.Contains(roleName)).ToArray();
            if (roles.Length == 0)
            {
                return IdentityResult.Success;
            }

            return await AddToRolesAsync(user, roles);
        }

        public async Task<bool> CheckDuplicateMobilePhoneAsync(string phoneNumber)
        {
            //write phoneNumber to redis for uniqueness checking
            var cache = _cacheManager.GetCache(TenantContactMethodCacheName);
            if (cache.GetOrDefault(phoneNumber) == null)
            {
                cache.Set(phoneNumber, 1);
            }
            else
            {
                throw new UserFriendlyException(string.Format(L("PhoneNumberDuplicated"),phoneNumber));
            }
            return await _store.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task<bool> CheckDuplicateMobilePhoneInStoreAsync(string phoneNumber)
        {
            return await _store.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        //public async Task<bool> CheckDuplicateUsernameAsync(string usernanme)
        //{
        //    return await _store.Users.AnyAsync(u => u.UserName == usernanme);
        //}

        public async Task<bool> CheckDuplicateEmailAsync(string email)
        {
            var cache = _cacheManager.GetCache(TenantContactMethodCacheName);
            if (cache.GetOrDefault(email) == null)
            {
                cache.Set(email, 1);
            }
            else
            {
                throw new UserFriendlyException(string.Format(L("EmailAddressDuplicated"),email));
            }
            return await _store.Users.AnyAsync(u => u.EmailAddress == email);
        }
        public async Task<bool> CheckDuplicateEmailInStoreAsync(string email)
        {
            return await _store.Users.AnyAsync(u => u.EmailAddress == email);
        }

        public override async Task<IList<User>> GetUsersInRoleAsync(string roleName)
        {
            var role = await _roleManager.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                return new List<User>();
            }

            var users = await _store.GetUsersInRoleAsync(role.NormalizedName);

            return users;
        }

        public async Task<string[]> GetOrgUnitsOfUserAsync(User user)
        {
            var orgUnits = await GetOrganizationUnitsAsync(user);
            return orgUnits.Select(ou => ou.DisplayName).ToArray();
        }

        public async Task<string[]> GetRolesOfUserAsync(User user)
        {
            var roles = await GetRolesAsync(user);
            return roles.ToArray();
        }

        public async Task<string[]> GetPermissionsOfUserAsync(User user)
        {
            var permissions = await GetGrantedPermissionsAsync(user);
            return permissions.Select(p => p.Name).ToArray();
        }
    }
}
