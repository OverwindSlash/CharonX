using Abp.Organizations;

namespace CharonX.Organizations
{
    public static class OrganizationUnitHelper
    {
        public const string DefaultAdminOrgUnitName = "AdminGroup";

        public static OrganizationUnit CreateDefaultAdminOrgUnit(int tenantId)
        {
            OrganizationUnit adminOu = new OrganizationUnit()
            {
                TenantId = tenantId,
                ParentId = null,
                DisplayName = DefaultAdminOrgUnitName
            };

            return adminOu;
        }
    }
}
