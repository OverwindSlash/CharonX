using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Abp.Dependency;
using CharonX.EntityFrameworkCore;
using CharonX.Identity;
using CharonX.Authorization;

namespace CharonX.Tests.DependencyInjection
{
    public static class ServiceCollectionRegistrar
    {
        public static void Register(IIocManager iocManager)
        {
            var services = new ServiceCollection();

            IdentityRegistrar.Register(services);

            services.AddEntityFrameworkInMemoryDatabase();

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);

            var builder = new DbContextOptionsBuilder<CharonXDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).UseInternalServiceProvider(serviceProvider);

            iocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<CharonXDbContext>>()
                    .Instance(builder.Options)
                    .LifestyleSingleton()
            );
            //IocManager.Instance.IocContainer.Register(
            //    Component
            //        .For<DbContextOptions<CharonXDbContext>>()
            //        .Instance(builder.Options)
            //        .LifestyleSingleton()
            //);
            //IocManager.Instance.RegisterIfNot<IUnitOfWorkDefaultOptions, UnitOfWorkDefaultOptions>(DependencyLifeStyle.Transient);
            //IocManager.Instance.RegisterIfNot<ICurrentUnitOfWorkProvider, AsyncLocalCurrentUnitOfWorkProvider>(DependencyLifeStyle.Transient);
            //IocManager.Instance.IocContainer.Register(
            //    Component.For(typeof(IDbContextProvider<>))
            //        .ImplementedBy(typeof(UnitOfWorkDbContextProvider<>))
            //        .LifestyleTransient()
            //    );
            //IocManager.Instance.RegisterIfNot<IDbContextProvider<CharonXDbContext>, UnitOfWorkDbContextProvider<>>();
            //Abp.Dependency.IocManager.Instance.IocContainer.Register(Component.For<IRepository<CustomPermissionSetting>>().ImplementedBy<EfCoreRepositoryBase<CharonXDbContext, CustomPermissionSetting>>().LifestyleTransient());

            //IocManager.Instance.RegisterIfNot<IRepository<CustomPermissionSetting>, EfCoreRepositoryBase<CharonXDbContext, CustomPermissionSetting>>(DependencyLifeStyle.Transient);

            IocManager.Instance.RegisterIfNot<IJustForUnitTest, JustForUnitTest>(DependencyLifeStyle.Transient);
        }
    }
}
