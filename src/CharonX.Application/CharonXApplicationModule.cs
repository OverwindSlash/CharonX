using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using CharonX.Authorization;
using CharonX.Authorization.Users;
using CharonX.Users.Dto;

namespace CharonX
{
    [DependsOn(
        typeof(CharonXCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CharonXApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CharonXAuthorizationProvider>();

            // Ignore permission member when mapping UserDto to User
            Configuration.Modules.AbpAutoMapper().Configurators.Add(config =>
            {
                config.CreateMap<UserDto, User>()
                    .ForMember(t => t.Permissions,
                        options => options.Ignore());
            });
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CharonXApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
