using Abp.Application.Services;
using CharonX.Users.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace CharonX.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<List<UserDto>> GetUsersInRoleAsync(string roleName);

        Task<List<UserDto>> GetUsersInOrgUnitAsync(string orgUnitName, bool includeChildren = false);

        //Task<ListResultDto<RoleDto>> GetRoles();

        Task<ListResultDto<string>> GetPermissions(GetPermissionsDto input);

        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);

        Task<bool> ActivateUser(ActivateUserDto input);

        Task<bool> CheckAvailableOfPhoneNumber(string phoneNumber);

        Task<bool> CheckAvailableOfEmailAddress(string emailAddress);
    }
}
