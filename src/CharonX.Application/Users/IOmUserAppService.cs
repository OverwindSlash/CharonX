using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CharonX.Users.Dto;

namespace CharonX.Users
{
    public interface IOmUserAppService
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
