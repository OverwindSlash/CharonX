using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using System.Threading.Tasks;
using CharonX.Features.Dto;
using CharonX.Users.Dto;

namespace CharonX.Roles
{
    public interface IRoleAppService : IAsyncCrudAppService<RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>
    {
        Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input);

        Task<ListResultDto<RoleListDto>> GetRolesByPermissionAsync(GetRolesInput input);

        Task AddUserToRoleAsync(SetRoleUserDto input);

        Task RemoveUserFromRoleAsync(SetRoleUserDto input);

        Task<List<UserDto>> GetUsersInRoleAsync(EntityDto<int> input);

        ListResultDto<PermissionDto> GetAllAvailablePermissions();
    }
}
