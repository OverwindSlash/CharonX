using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CharonX.Configuration;
using CharonX.Web;

namespace CharonX.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class CharonXDbContextFactory : IDesignTimeDbContextFactory<CharonXDbContext>
    {
        public CharonXDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CharonXDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            CharonXDbContextConfigurer.Configure(builder, configuration.GetConnectionString(CharonXConsts.ConnectionStringName));

            return new CharonXDbContext(builder.Options);
        }
    }
}
