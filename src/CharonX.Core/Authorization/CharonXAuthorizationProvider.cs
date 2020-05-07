//#define TEST_IGNORE
using Abp.Application.Features;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Abp.MultiTenancy;
using CharonX.Authorization.Users;
using CharonX.Entities;
using CharonX.Features;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace CharonX.Authorization
{
    public class CharonXAuthorizationProvider : AuthorizationProvider
    {
        private readonly IRepository<CustomPermissionSetting> permissionRepository;
        private readonly IIocResolver _iocResolver;
        private bool testIgnore=false;
        public CharonXAuthorizationProvider(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
            permissionRepository = _iocResolver.Resolve<IRepository<CustomPermissionSetting>>();
            //customPermissionRepository = IocManager.Instance.Resolve<ICustomPermissionRepository>();

            if (IocManager.Instance.IsRegistered<IJustForUnitTest>())
            {
                testIgnore = true;
            }
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            // Abp native permissions
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            // Business features permissions
            // For smart security
            var smartSecurityPermission = context.CreatePermission("SmartSecurity",
                L("SmartSecurity"),
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartSecurityFeature));

            // For smart pass
            var smartPassPermission = context.CreatePermission("SmartPass",
                L("SmartPass"),
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));

            //read from db
            var permissions = GetAllPermissionsFromDb();
            foreach (var permission in permissions)
            {
                if (!string.IsNullOrEmpty(permission.FeatureDependency))
                {
                    context.CreatePermission(permission.Name,
                        L(permission.Name),
                        featureDependency: new SimpleFeatureDependency(permission.FeatureDependency));
                }
                else
                {
                    context.CreatePermission(permission.Name,
                        L(permission.Name));
                }
            }
#if false
            

            // var getAuthGroupPermission = context.CreatePermission("app:authgroup:getAuthGroup",
            //     featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));
#endif

        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, CharonXConsts.LocalizationSourceName);
        }
#if false
        private List<CustomPermissionSetting> GetAllPermissionsFromFile()
        {
            var basePath = Path.GetDirectoryName(typeof(CharonXAuthorizationProvider).Assembly.Location);
            var jsonFile = Path.Combine(basePath, "permissionsetting.json");

            var json = File.ReadAllText(jsonFile);
            var permissions = JsonConvert.DeserializeObject<List<CustomPermissionSetting>>(json);

            return permissions;
        }
#endif

        private List<CustomPermissionSetting> GetAllPermissionsFromDb()
        {
            var settings = permissionRepository.GetAllList();
            return settings;
        }
    }
}
