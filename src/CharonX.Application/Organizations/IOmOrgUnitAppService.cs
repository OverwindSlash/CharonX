using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Organizations.Dto;
using CharonX.Roles.Dto;

namespace CharonX.Organizations
{
    public interface IOmOrgUnitAppService : IApplicationService
    {
        public Task<OrgUnitDto> CreateOrgUnitInTenantAsync(int tenantId, CreateOrgUnitDto input);
        public Task<OrgUnitDto> GetOrgUnitInTenantAsync(int tenantId, EntityDto<int> input);
        public Task<ListResultDto<OrgUnitListDto>> GetAllOrgUnitInTenantAsync(int tenantId, GetOrgUnitsInput input);
        public Task<OrgUnitDto> UpdateOrgUnitInTenantAsync(int tenantId, OrgUnitDto input);
        public Task DeleteOrgUnitInTenantAsync(int tenantId, EntityDto<int> input);
    }
}
