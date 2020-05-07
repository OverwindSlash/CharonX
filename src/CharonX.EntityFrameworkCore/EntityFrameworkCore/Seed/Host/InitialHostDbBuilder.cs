using Abp.Dependency;

namespace CharonX.EntityFrameworkCore.Seed.Host
{
    public class InitialHostDbBuilder
    {
        private readonly CharonXDbContext _context;
        private readonly IIocResolver _iocResolver;
        public InitialHostDbBuilder(CharonXDbContext context, IIocResolver iocResolver)
        {
            _context = context;
            _iocResolver = iocResolver;
        }

        public void Create()
        {
            new DefaultEditionCreator(_context).Create();
            new DefaultLanguagesCreator(_context).Create();
            new HostRoleAndUserCreator(_context,_iocResolver).Create();
            new DefaultSettingsCreator(_context).Create();

            _context.SaveChanges();
        }
    }
}
