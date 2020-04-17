using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using CharonX.Authorization;
using CharonX.Roles.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.IdentityFramework;
using CharonX.Authorization.Roles;
using Microsoft.AspNetCore.Identity;

namespace CharonX.Roles
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class OmRoleAppService : ApplicationService, IOmRoleAppService
    {
        private readonly RoleManager _roleManager;

        public OmRoleAppService(RoleManager roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input)
        {
            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await _roleManager.CreateAndGrantPermissionAsync(role, input.GrantedPermissions);
            }

            return ObjectMapper.Map<RoleDto>(role);
        }

        public Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            throw new NotImplementedException();
        }

        public Task<ListResultDto<RoleListDto>> GetAllRolesInTenantAsync(int tenantId, GetRolesInput input)
        {
            throw new NotImplementedException();
        }

        public Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            throw new NotImplementedException();
        }

        
    }
}
