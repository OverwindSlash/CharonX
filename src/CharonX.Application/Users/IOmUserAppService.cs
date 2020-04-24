using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Users.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharonX.Users
{
    public interface IOmUserAppService : IApplicationService
    {
        public Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input);

        public Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input);

        public Task<List<UserDto>> GetAllAdminUserInTenantAsync(int tenantId);

        public Task<UserDto> GetUserInTenantAsync(int tenantId, EntityDto<long> input);

        public Task<UserDto> UpdateUserInTenantAsync(int tenantId, UserDto input);

        public Task DeleteUserInTenantAsync(int tenantId, EntityDto<long> input);

        public Task<bool> ActivateUserInTenantAsync(int tenantId, ActivateUserDto input);
    }
}
