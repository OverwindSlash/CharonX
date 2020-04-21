using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Organizations.Dto;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharonX.Organizations
{
    public interface IOrgUnitAppService : IApplicationService
    {
        public Task<OrgUnitDto> CreateAsync(CreateOrgUnitDto input);

        public Task<OrgUnitDto> GetAsync(EntityDto<long> input);

        public Task<PagedResultDto<OrgUnitDto>> GetAllAsync(PagedResultRequestDto input);

        public Task<OrgUnitDto> UpdateAsync(OrgUnitDto input);

        public Task DeleteAsync(EntityDto<long> input);

        public Task AddRoleToOrgUnitAsync(SetOrgUnitRoleDto input);
        public Task RemoveRoleFromOrgUnitAsync(SetOrgUnitRoleDto input);

        public Task<List<RoleDto>> GetRolesInOrgUnitAsync(EntityDto<long> input, bool includeChildren = false);

        public Task AddUserToOrgUnitAsync(SetOrgUnitUserDto input);
        public Task RemoveUserFromOrgUnitAsync(SetOrgUnitUserDto input);

        public Task<List<UserDto>> GetUsersInOrgUnitAsync(EntityDto<long> input, bool includeChildren = false);
    }
}
