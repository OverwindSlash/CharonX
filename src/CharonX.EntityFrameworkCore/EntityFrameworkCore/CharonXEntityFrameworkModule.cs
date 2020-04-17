using System.Transactions;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using CharonX.EntityFrameworkCore.Seed;

namespace CharonX.EntityFrameworkCore
{
    [DependsOn(
        typeof(CharonXCoreModule), 
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class CharonXEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            // For TiDB
            Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;

            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<CharonXDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        CharonXDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        CharonXDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(CharonXEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}
