using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
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
            _orgUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _organizationUnitSettings = organizationUnitSettings;
        }

        public async Task<IdentityResult> AddToOrgUnitsAsync(User user, string[] orgUnitNames)
        {
            if (user == null || orgUnitNames == null)
            {
                return IdentityResult.Failed();
            }

            var ous = _orgUnitRepository.GetAll()
                .Where(ou => orgUnitNames.Contains(ou.DisplayName)).ToList();

            foreach (OrganizationUnit ou in ous)
            {
                await AddToOrganizationUnitAsync(user, ou);
            }

            return IdentityResult.Success;
        }

        public async Task<List<string>> GetRolesInOrgUnitsAsync(string[] orgUnitNames)
        {
            List<string> roleNames = new List<string>();

            var organizationUnits = await _orgUnitRepository.GetAll()
                .Where(ou => orgUnitNames.Contains(ou.DisplayName)).ToListAsync();

            foreach (var orgUnit in organizationUnits)
            {
                var roles = await _roleManager.GetRolesInOrganizationUnit(orgUnit);

                foreach (Role role in roles)
                {
                    if (roleNames.Contains(role.Name))
                    {
                        continue;
                    }

                    roleNames.Add(role.Name);
                }
            }

            return roleNames;
        }
    }
}
