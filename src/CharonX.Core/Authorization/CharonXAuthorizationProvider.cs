using Abp.Application.Features;
using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;
using CharonX.Features;

namespace CharonX.Authorization
{
    public class CharonXAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            // Abp native permissions
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            // Business features permissions
            var smartSecurityPermission = context.CreatePermission("SmartSecurity",
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartSecurityFeature));

            var smartPassPermission = context.CreatePermission("SmartPass",
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, CharonXConsts.LocalizationSourceName);
        }
    }
}
