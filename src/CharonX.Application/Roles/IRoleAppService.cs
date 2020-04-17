using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using System.Threading.Tasks;

namespace CharonX.Roles
{
    public interface IRoleAppService : IAsyncCrudAppService<RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>
    {
        Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input);

        Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input);
    }
}
