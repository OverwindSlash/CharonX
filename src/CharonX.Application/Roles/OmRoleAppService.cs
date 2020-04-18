using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Extensions;
using Abp.Linq.Extensions;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<ListResultDto<RoleListDto>> GetAllRolesInTenantAsync(int tenantId, GetRolesInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var roles = await _roleManager
                    .Roles
                    .WhereIf(
                        !input.Permission.IsNullOrWhiteSpace(),
                        r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                    )
                    .ToListAsync();

                return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
            }
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
