using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Organizations.Dto;
using System.Threading.Tasks;
using CharonX.Roles.Dto;

namespace CharonX.Organizations
{
    public interface IOmOrgUnitAppService : IApplicationService
    {
        public Task<OrgUnitDto> CreateOrgUnitInTenantAsync(int tenantId, CreateOrgUnitDto input);

        public Task<OrgUnitDto> GetOrgUnitInTenantAsync(int tenantId, EntityDto<long> input);

        public Task<ListResultDto<OrgUnitListDto>> GetAllOrgUnitInTenantAsync(int tenantId, GetOrgUnitsInput input);

        public Task<OrgUnitDto> UpdateOrgUnitInTenantAsync(int tenantId, OrgUnitDto input);

        public Task DeleteOrgUnitInTenantAsync(int tenantId, EntityDto<long> input);

        public Task AddRoleToOrgUnitInTenantAsync(int tenantId, SetOrgUnitRoleDto input);

        public Task RemoveRoleFromOrgUnitInTenantAsync(int tenantId, SetOrgUnitRoleDto input);

        public Task<List<RoleDto>> GetRolesInOrgUnitInTenantAsync(int tenantId, EntityDto<long> input, bool includeChildren = false);
    }
}
