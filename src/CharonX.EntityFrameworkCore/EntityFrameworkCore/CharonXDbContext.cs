using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;
using Abp.Authorization;
using CharonX.Entities;

namespace CharonX.EntityFrameworkCore
{
    public class CharonXDbContext : AbpZeroDbContext<Tenant, Role, User, CharonXDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public virtual DbSet<CustomPermissionSetting> CustomPermissionSettings { get; set; }
        public virtual DbSet<CustomFeatureSetting> CustomFeatureSettings { get; set; }
        public CharonXDbContext(DbContextOptions<CharonXDbContext> options)
            : base(options)
        {
        }
    }
}
