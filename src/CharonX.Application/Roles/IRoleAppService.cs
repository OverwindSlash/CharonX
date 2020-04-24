using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using System.Threading.Tasks;
using CharonX.Users.Dto;

namespace CharonX.Roles
{
    public interface IRoleAppService : IAsyncCrudAppService<RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>
    {
        Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input);

        Task<ListResultDto<RoleListDto>> GetRolesByPermissionAsync(GetRolesInput input);

        public Task AddUserToRoleAsync(SetRoleUserDto input);

        public Task RemoveUserFromRoleAsync(SetRoleUserDto input);

        public Task<List<UserDto>> GetUsersInRoleAsync(EntityDto<int> input);
    }
}
