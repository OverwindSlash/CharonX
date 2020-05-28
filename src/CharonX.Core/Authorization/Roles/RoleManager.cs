using Abp.Application.Features;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.MultiTenancy;
using Abp.Organizations;
using Abp.Runtime.Caching;
using Abp.Zero.Configuration;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharonX.Authorization.Roles
{
    public class RoleManager : AbpRoleManager<Role, User>
    {
        private readonly IPermissionManager _permissionManager;
        private readonly IIocManager _iocManager;

        public RoleManager(
            RoleStore store,
            IEnumerable<IRoleValidator<Role>> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<AbpRoleManager<Role, User>> logger,
            IPermissionManager permissionManager,
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRoleManagementConfig roleManagementConfig,
            IRepository<OrganizationUnit, long> organizationUnitRepository,
            IRepository<OrganizationUnitRole, long> organizationUnitRoleRepository,
            IIocManager iocManager)
            : base(store, roleValidators, keyNormalizer, errors, logger, 
                permissionManager, cacheManager, unitOfWorkManager, roleManagementConfig,
                organizationUnitRepository, organizationUnitRoleRepository)
        {
            _permissionManager = permissionManager;
            _iocManager = iocManager;
        }

        public async Task GrantAllPermissionToAdminRoleInTenant(Tenant tenant)
        {
            var adminRole = Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
            await GrantAllPermissionsAsync(adminRole);

            //using (var featureDependencyContext = _iocManager.ResolveAsDisposable<FeatureDependencyContext>())
            //{
            //    var featureDependencyContextObject = featureDependencyContext.Object;
            //    featureDependencyContextObject.TenantId = tenant.Id;

            //    var permissions = _permissionManager.GetAllPermissions(adminRole.GetMultiTenancySide()).ToList();

            //    permissions = permissions.Where(permission =>
            //            permission.FeatureDependency == null ||
            //            permission.FeatureDependency.IsSatisfied(featureDependencyContextObject)
            //        ).ToList();

            //    await SetGrantedPermissionsAsync(adminRole, permissions);
            //}
        }

        public async Task CreateAndGrantPermissionAsync(Role role, List<string> permissions)
        {
            CheckErrors(await CreateAsync(role));

            var grantedPermissions = _permissionManager.GetAllPermissions()
                .Where(p => permissions.Contains(p.Name))
                .ToList();

            await SetGrantedPermissionsAsync(role, grantedPermissions);
        }

        public async Task UpdateRoleAndPermissionAsync(Role role, List<string> permissions)
        {
            await ResetAllPermissionsAsync(role);

            CheckErrors(await UpdateAsync(role));

            var grantedPermissions = _permissionManager.GetAllPermissions()
                .Where(p => permissions.Contains(p.Name))
                .ToList();

            await SetGrantedPermissionsAsync(role, grantedPermissions);
        }

        public async Task DeleteRoleAndDetachUserAsync(Role role)
        {
            UserManager userManager = _iocManager.Resolve<UserManager>();

            var users = await userManager.GetUsersInRoleAsync(role.NormalizedName);

            foreach (var user in users)
            {
                CheckErrors(await userManager.RemoveFromRoleAsync(user, role.NormalizedName));
            }

            CheckErrors(await DeleteAsync(role));
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
