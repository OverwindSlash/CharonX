using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharonX.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<List<UserDto>> GetUsersInRoleAsync(string roleName);

        Task<List<UserDto>> GetUsersInOrgUnitAsync(string orgUnitName, bool includeChildren = false);

        //Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);

        Task<bool> ActivateUser(ActivateUserDto input);
    }
}
