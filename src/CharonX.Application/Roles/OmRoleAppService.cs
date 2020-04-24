using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.MultiTenancy;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharonX.Authorization.Users;
using CharonX.Users.Dto;

namespace CharonX.Roles
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class OmRoleAppService : ApplicationService, IOmRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;

        public OmRoleAppService(
            RoleManager roleManager,
            TenantManager tenantManager,
            UserManager userManager)
        {
            _roleManager = roleManager;
            _tenantManager = tenantManager;
            _userManager = userManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public async Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input)
        {
            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            var tenant = await _tenantManager.GetAvailableTenantById(tenantId);

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await _roleManager.CreateAndGrantPermissionAsync(role, input.GrantedPermissions);
            }

            return ObjectMapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var role = await _roleManager.GetRoleByIdAsync(input.Id);
                    var permissions = await _roleManager.GetGrantedPermissionsAsync(role);
                    
                    RoleDto dto = ObjectMapper.Map<RoleDto>(role);
                    dto.GrantedPermissions = permissions.Select(p => p.Name).ToList();

                    return dto;
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }

        public async Task<ListResultDto<RoleListDto>> GetRolesByPermissionInTenantAsync(int tenantId, GetRolesInput input)
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

        public async Task<PagedResultDto<RoleDto>> GetAllRolesInTenantAsync(int tenantId, PagedRoleResultRequestDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var query = _roleManager.Roles;
                var totalCount = await _roleManager.Roles.CountAsync();

                query = PagingHelper.ApplySorting<Role, int>(query, input);
                query = PagingHelper.ApplyPaging<Role, int>(query, input);

                var entities = await query.ToListAsync();

                return new PagedResultDto<RoleDto>(totalCount, entities.Select(
                        r => ObjectMapper.Map<RoleDto>(r)).ToList()
                );
            }
        }

        public async Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var role = await _roleManager.GetRoleByIdAsync(input.Id);

                    ObjectMapper.Map(input, role);

                    await _roleManager.UpdateRoleAndPermissionAsync(role, input.GrantedPermissions);

                    return ObjectMapper.Map<RoleDto>(role);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }

        public async Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var role = await _roleManager.FindByIdAsync(input.Id.ToString());
                if (role == null)
                {
                    throw new UserFriendlyException(L("RoleNotFound", input.Id));
                }

                await _roleManager.DeleteRoleAndDetachUserAsync(role);
            }
        }

        public async Task AddUserToRoleInTenantAsync(int tenantId, SetRoleUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(input.UserId);
                    var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }

        public async Task RemoveUserFromRoleInTenantAsync(int tenantId, SetRoleUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(input.UserId);
                    var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }

        public async Task<List<UserDto>> GetUsersInRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var role = await _roleManager.GetRoleByIdAsync(input.Id);

                if (role == null)
                {
                    throw new UserFriendlyException(L("RoleNotFound", input.Id));
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name);

                return ObjectMapper.Map<List<UserDto>>(users);
            }
        }
    }
}
