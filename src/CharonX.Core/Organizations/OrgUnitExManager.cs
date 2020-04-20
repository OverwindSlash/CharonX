using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Organizations;

namespace CharonX.Organizations
{
    public class OrgUnitExManager : OrganizationUnitManager
    {
        public OrgUnitExManager(
            IRepository<OrganizationUnit, long> organizationUnitRepository
            ) : base(organizationUnitRepository)
        {
        }

        public override Task CreateAsync(OrganizationUnit organizationUnit)
        {
            return base.CreateAsync(organizationUnit);
        }
    }
}
