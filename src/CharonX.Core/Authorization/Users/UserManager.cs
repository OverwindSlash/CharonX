using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.Organizations;
using Abp.Runtime.Caching;
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
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IOrganizationUnitSettings _organizationUnitSettings;

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
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitSettings = organizationUnitSettings;

            LocalizationSourceName = "CharonX";
        }

        public async Task<IdentityResult> SetOrgUnitsAsync(User user, string[] orgUnitNames)
        {
            if (user == null || orgUnitNames == null)
            {
                return IdentityResult.Failed();
            }

            var organizationUnits = _orgUnitRepository.GetAll()
                .Where(ou => orgUnitNames.Contains(ou.DisplayName)).ToList();

            await SetOrganizationUnitsAsync(user, organizationUnits.Select(ou => ou.Id).ToArray());

            List<Role> roles = new List<Role>();
            foreach (OrganizationUnit organizationUnit in organizationUnits)
            {
                roles.AddRange(await _roleManager.GetRolesInOrganizationUnit(organizationUnit));
            }
            roles = roles.Distinct().ToList();
            await SetRolesAsync(user, roles.Select(r => r.Name).ToArray());

            return IdentityResult.Success;
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
            return await _store.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
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
    }
}
