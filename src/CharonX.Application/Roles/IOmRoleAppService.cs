using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using System.Threading.Tasks;

namespace CharonX.Roles
{
    public interface IOmRoleAppService : IApplicationService
    {
        public Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input);
        public Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input);
        public Task<ListResultDto<RoleListDto>> GetAllRolesInTenantAsync(int tenantId, GetRolesInput input);
        public Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input);
        public Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input);
    }
}
