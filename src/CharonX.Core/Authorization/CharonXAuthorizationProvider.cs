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
            // For smart security
            var smartSecurityPermission = context.CreatePermission("SmartSecurity",
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartSecurityFeature));

            // For smart pass
            var smartPassPermission = context.CreatePermission("SmartPass",
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));

            // var getAuthGroupPermission = context.CreatePermission("app:authgroup:getAuthGroup",
            //     featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));

            var smartPassPermissions = new string[] { "common:upload:image","common:upload:video","common:app:upgrade","common:message:send",
                "common:message:verification","tenant:employee:info","tenant:employee:save","tenant:employee:update","tenant:employee:delete",
                "tenant:employee:list","app:blacklist:list","app:blacklist:info","app:blacklist:save","app:blacklist:update","app:blacklist:delete",
                "app:authgroup:list","app:authgroup:info","app:authgroup:save","app:authgroup:update","app:authgroup:delete","app:authgroup:getAuthGroup",
                "app:groupemployee:save","app:groupemployee:delete","app:groupemployee:list","app:groupemployee:info","app:groupemployee:update","app:visitor:list",
                "app:visitor:info","app:visitor:save","app:visitor:update","app:visitor:delete","app:visitor:getPurpose","tenant:devicegroup:list",
                "tenant:devicegroup:info","tenant:devicegroup:save","tenant:devicegroup:update","tenant:devicegroup:delete","tenant:alarm:update",
                "tenant:alarm:list","tenant:alarm:info","tenant:alarm:getAlarmType","tenant:dept:list","tenant:dept:info","tenant:dept:save",
                "tenant:dept:update","tenant:dept:delete","tenant:compare:list","tenant:compare:update","tenant:compare:info","tenant:device:list",
                "tenant:device:info","tenant:device:getDeviceType","tenant:device:upgradeSoft","tenant:device:openDoor","tenant:device:synLibrary",
                "tenant:device:reboot","tenant:accesstimetable:list","tenant:accesstimetable:info","tenant:accesstimetable:save","tenant:accesstimetable:update",
                "tenant:accesstimetable:delete","tenant:accesstimetable:queryAllAccessModeList","tenant:strategy:list","tenant:strategy:info","tenant:strategy:save",
                "tenant:strategy:update","tenant:strategy:delete","tenant:deviceconfigure:push","tenant:deviceconfigure:info","tenant:deviceconfigure:update",
                "tenant:devicemonitor:info","tenant:devicemonitor:push","app:groupVisitor:delete","app:groupVisitor:list","app:groupVisitor:save",
                "app:groupblacklist:list","app:groupblacklist:info","app:groupblacklist:save","app:groupblacklist:update","app:groupblacklist:delete" };

            foreach (var permission in smartPassPermissions)
            {
                context.CreatePermission(permission,featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));
            }
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, CharonXConsts.LocalizationSourceName);
        }
    }
}
