using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Organizations.Dto;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharonX.Organizations
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class OmOrgUnitAppService : ApplicationService, IOmOrgUnitAppService
    {
        private readonly OrganizationUnitManager _orgUnitManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly IRepository<OrganizationUnitRole, long> _orgUnitRoleRepository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;

        public OmOrgUnitAppService(
            OrganizationUnitManager orgUnitManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            IRepository<OrganizationUnitRole, long> orgUnitRoleRepository,
            RoleManager roleManager,
            UserManager userManager)
        {
            _orgUnitManager = orgUnitManager;
            _orgUnitRepository = orgUnitRepository;
            _orgUnitRoleRepository = orgUnitRoleRepository;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        /// <summary>
        /// 运维专用：创建特定租户下的组织
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> CreateOrgUnitInTenantAsync(int tenantId, CreateOrgUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = ObjectMapper.Map<OrganizationUnit>(input);
                orgUnit.TenantId = tenantId;

                await _orgUnitManager.CreateAsync(orgUnit);
                //await CurrentUnitOfWork.SaveChangesAsync();

                return await GenerateOrgUnitDtoAsync(orgUnit);
            }
        }

        private async Task<OrgUnitDto> GenerateOrgUnitDtoAsync(OrganizationUnit orgUnit)
        {
            var orgUnitDto = ObjectMapper.Map<OrgUnitDto>(orgUnit);
            var roles = await _roleManager.GetRolesInOrganizationUnit(orgUnit);
            orgUnitDto.AssignedRoles = roles.Select(r => r.Name).ToList();

            List<Permission> permissions = new List<Permission>();
            foreach (Role role in roles)
            {
                permissions.AddRange(await _roleManager.GetGrantedPermissionsAsync(role));
            }
            permissions = permissions.Distinct().ToList();
            orgUnitDto.GrantedPermissions = permissions.Select(p => p.Name).ToList();

            return orgUnitDto;
        }

        /// <summary>
        /// 运维专用：获取特定租户下的指定组织
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> GetOrgUnitInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

                return await GenerateOrgUnitDtoAsync(orgUnit);
            }
        }

        /// <summary>
        /// 运维专用：获取特定租户下的所有组织
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<OrgUnitDto>> GetAllOrgUnitInTenantAsync(int tenantId, GetOrgUnitsInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var allOrgUnits = await _orgUnitRepository.GetAll().ToListAsync();

                if (!string.IsNullOrEmpty(input.Role))
                {
                    var role = await _roleManager.GetRoleByNameAsync(input.Role);
                    var orgUnitRoles = _orgUnitRoleRepository.GetAll()
                        .Where(our => our.RoleId == role.Id);

                    var excludeOrgUnitIds = await orgUnitRoles.Select(our => our.OrganizationUnitId).ToListAsync();

                    var excludeOrgUnits = allOrgUnits.Where(ou => excludeOrgUnitIds.Contains(ou.Id)).ToList();

                    foreach (OrganizationUnit orgUnit in excludeOrgUnits)
                    {
                        allOrgUnits.Remove(orgUnit);
                    }
                }

                List<OrgUnitDto> orgUnitDtos = new List<OrgUnitDto>();
                foreach (OrganizationUnit orgUnit in allOrgUnits)
                {
                    orgUnitDtos.Add(await GenerateOrgUnitDtoAsync(orgUnit));
                }

                return new ListResultDto<OrgUnitDto>(orgUnitDtos);
            }
        }

        /// <summary>
        /// 运维专用：更新特定租户下的某个组织
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> UpdateOrgUnitInTenantAsync(int tenantId, OrgUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = await _orgUnitRepository
                    .GetAllIncluding(ou => ou.Children)
                    .FirstOrDefaultAsync(ou => ou.Id == input.Id);

                if (orgUnit.ParentId != input.ParentId)
                {
                    await _orgUnitManager.MoveAsync(orgUnit.Id, input.ParentId);
                }

                orgUnit.DisplayName = input.DisplayName;

                await _orgUnitManager.UpdateAsync(orgUnit);

                return ObjectMapper.Map<OrgUnitDto>(orgUnit);
            }
        }
        /// <summary>
        /// 运维专用：删除特定租户下的某一组织
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task DeleteOrgUnitInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await _orgUnitManager.DeleteAsync(input.Id);
            }
        }
        /// <summary>
        /// 运维专用：对特定租户下的指定组织添加一个角色
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddRoleToOrgUnitInTenantAsync(int tenantId, SetOrgUnitRoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                    var ou = await _orgUnitRepository.GetAsync(input.OrgUnitId);
                    await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrgUnitId, tenantId);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }
        /// <summary>
        /// 运维专用：对特定租户下的指定组织删除一个角色
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task RemoveRoleFromOrgUnitInTenantAsync(int tenantId, SetOrgUnitRoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                    var ou = await _orgUnitRepository.GetAsync(input.OrgUnitId);
                    await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrgUnitId);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }
        /// <summary>
        /// 运维专用：获取特定租户下指定组织所包含的全部角色
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public async Task<List<RoleDto>> GetRolesInOrgUnitInTenantAsync(int tenantId, EntityDto<long> input,
            bool includeChildren = false)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

                if (orgUnit == null)
                {
                    throw new UserFriendlyException(L("OrgUnitNotFound", input.Id));
                }

                var roles = await _roleManager.GetRolesInOrganizationUnit(orgUnit, includeChildren);

                List<RoleDto> roleDtos = ObjectMapper.Map<List<RoleDto>>(roles);
                foreach (Role role in roles)
                {
                    int offset = roles.IndexOf(role);
                    var permissions = await _roleManager.GetGrantedPermissionsAsync(role);
                    roleDtos[offset].GrantedPermissions = permissions.Select(p => p.Name).ToList();
                }

                return roleDtos;
            }
        }
        /// <summary>
        /// 运维专用：对特定租户下指定组织添加一个用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddUserToOrgUnitInTenantAsync(int tenantId, SetOrgUnitUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfUserAndOrgUnitAsync(input);

                await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrgUnitId);
            }
        }

        private async Task CheckExistenceOfUserAndOrgUnitAsync(SetOrgUnitUserDto input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                var ou = await _orgUnitRepository.GetAsync(input.OrgUnitId);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }
        /// <summary>
        /// 运维专用：对特定租户下的指定组织删除某一用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task RemoveUserFromOrgUnitInTenantAsync(int tenantId, SetOrgUnitUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfUserAndOrgUnitAsync(input);

                await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrgUnitId);
            }
        }
        /// <summary>
        /// 运维专用：获取特定租户下指定组织的全部用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersInOrgUnitInTenantAsync(int tenantId, EntityDto<long> input, bool includeChildren = false)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

                if (orgUnit == null)
                {
                    throw new UserFriendlyException(L("OrgUnitNotFound", input.Id));
                }

                var users = await _userManager.GetUsersInOrganizationUnitAsync(orgUnit, includeChildren);

                return ObjectMapper.Map<List<UserDto>>(users);
            }
        }
    }
}
