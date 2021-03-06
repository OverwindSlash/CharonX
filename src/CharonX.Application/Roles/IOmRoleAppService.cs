﻿using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Roles.Dto;
using System.Threading.Tasks;
using CharonX.Users.Dto;

namespace CharonX.Roles
{
    public interface IOmRoleAppService : IApplicationService
    {
        public Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input);

        public Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input);

        public Task<ListResultDto<RoleListDto>> GetRolesByPermissionInTenantAsync(int tenantId, GetRolesInput input);

        public Task<PagedResultDto<RoleDto>> GetAllRolesInTenantAsync(int tenantId, PagedRoleResultRequestDto input);

        public Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input);

        public Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input);

        public Task AddUserToRoleInTenantAsync(int tenantId, SetRoleUserDto input);

        public Task RemoveUserFromRoleInTenantAsync(int tenantId, SetRoleUserDto input);

        public Task<List<UserDto>> GetUsersInRoleInTenantAsync(int tenantId, EntityDto<int> input);
    }
}
