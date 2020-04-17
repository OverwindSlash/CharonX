using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace CharonX.EntityFrameworkCore
{
    public static class CharonXDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CharonXDbContext> builder, string connectionString)
        {
            builder.UseMySql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<CharonXDbContext> builder, DbConnection connection)
        {
            builder.UseMySql(connection);
        }
    }
}
