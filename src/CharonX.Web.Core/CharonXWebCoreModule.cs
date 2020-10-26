using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.Configuration;
using CharonX.Authentication.JwtBearer;
using CharonX.Configuration;
using CharonX.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using CharonX.Authorization.AuthCode;
using Abp.Runtime.Caching.Redis;
using CharonX.MultiTenancy;
using CharonX.Roles;

namespace CharonX
{
    [DependsOn(
         typeof(AbpRedisCacheModule),
         typeof(CharonXApplicationModule),
         typeof(CharonXEntityFrameworkModule),
         typeof(AbpAspNetCoreModule)
        ,typeof(AbpAspNetCoreSignalRModule)
     )]
    public class CharonXWebCoreModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public CharonXWebCoreModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                CharonXConsts.ConnectionStringName
            );

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(CharonXApplicationModule).GetAssembly()
                 );

            ConfigureTokenAuth();
            Configuration.Caching.Configure(SmsAuthManager.SmsAuthCodeCacheName, cache =>
            {
                var expireMinutesStr = _appConfiguration.GetSection(SmsAuthManager.SmsAuthCodeCacheName)["ExpireMinutes"];
                int expireMinutes = 5;
                if (Int32.TryParse(expireMinutesStr, out var result))
                {
                    expireMinutes = result;
                }
                cache.DefaultSlidingExpireTime = TimeSpan.FromMinutes(expireMinutes);
            });

            Configuration.Caching.Configure("TenantContactMethod", cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromSeconds(10);
            });

            Configuration.Caching.Configure(TenantAppService.TenantAdminCacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromDays(1);
            });

            Configuration.Caching.Configure(RoleAppService.RolePermissionCacheName, cache =>
            {
                cache.DefaultSlidingExpireTime = TimeSpan.FromDays(1);
            });

            Configuration.Caching.UseRedis(options =>
            {
                options.ConnectionString = _appConfiguration["RedisCache:ConnectionString"];
                options.DatabaseId = _appConfiguration.GetValue<int>("RedisCache:DatabaseId");
            });
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials = new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            var expireDaysStr = _appConfiguration.GetSection("Authentication").GetSection("JwtBearer")["ExpireDays"];
            double expireDays = 1;
            if (double.TryParse(expireDaysStr, out var result))
            {
                expireDays = result;
            }
            tokenAuthConfig.Expiration = TimeSpan.FromDays(31);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CharonXWebCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(CharonXWebCoreModule).Assembly);
        }
    }
}
