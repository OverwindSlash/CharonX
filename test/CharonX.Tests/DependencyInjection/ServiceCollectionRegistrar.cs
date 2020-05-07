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
        private static object obj = new object();
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
            //just for unit test 
            lock (obj)
            {
                IocManager.Instance.RegisterIfNot<IJustForUnitTest, JustForUnitTest>(DependencyLifeStyle.Transient);
            }
        }
    }
}
