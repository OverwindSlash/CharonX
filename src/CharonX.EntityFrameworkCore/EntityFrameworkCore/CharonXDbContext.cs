using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;

namespace CharonX.EntityFrameworkCore
{
    public class CharonXDbContext : AbpZeroDbContext<Tenant, Role, User, CharonXDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public CharonXDbContext(DbContextOptions<CharonXDbContext> options)
            : base(options)
        {
        }
    }
}
