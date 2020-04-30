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
    [AbpAuthorize(PermissionNames.Pages_Roles)]
    public class OrgUnitAppService : ApplicationService, IOrgUnitAppService
    {
        private readonly OrganizationUnitManager _orgUnitManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;

        public OrgUnitAppService(
            OrganizationUnitManager orgUnitManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            RoleManager roleManager,
            UserManager userManager
            )
        {
            _orgUnitManager = orgUnitManager;
            _orgUnitRepository = orgUnitRepository;
            _roleManager = roleManager;
            _userManager = userManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }
        /// <summary>
        /// 创建当前租户下的组织
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> CreateAsync(CreateOrgUnitDto input)
        {
            var orgUnit = ObjectMapper.Map<OrganizationUnit>(input);
            orgUnit.TenantId = GetCurrentTenantId();

            await _orgUnitManager.CreateAsync(orgUnit);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GenerateOrgUnitDtoAsync(orgUnit);
        }
        /// <summary>
        /// 获取当前租户下的指定组织
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> GetAsync(EntityDto<long> input)
        {
            var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

            return await GenerateOrgUnitDtoAsync(orgUnit);
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
        /// 获取当前租户下的全部组织
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<OrgUnitDto>> GetAllAsync(PagedResultRequestDto input)
        {
            var query = _orgUnitRepository.GetAll();
            var totalCount = await _orgUnitRepository.CountAsync();

            query = PagingHelper.ApplySorting<OrganizationUnit, long>(query, input);
            query = PagingHelper.ApplyPaging<OrganizationUnit, long>(query, input);

            var entities = await query.ToListAsync();

            List<OrgUnitDto> orgUnitDtos = new List<OrgUnitDto>();
            foreach (OrganizationUnit orgUnit in entities)
            {
                orgUnitDtos.Add(await GenerateOrgUnitDtoAsync(orgUnit));
            }

            return new PagedResultDto<OrgUnitDto>(totalCount, orgUnitDtos);
        }
        /// <summary>
        /// 更新当前租户下的特定组织
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<OrgUnitDto> UpdateAsync(OrgUnitDto input)
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
        /// <summary>
        /// 删除当前租户下的特定组织
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task DeleteAsync(EntityDto<long> input)
        {
            await _orgUnitManager.DeleteAsync(input.Id);
        }
        /// <summary>
        /// 对当前租户下的特定组织添加一个角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddRoleToOrgUnitAsync(SetOrgUnitRoleDto input)
        {
            await CheckExistenceOfRoleAndOrgUnitAsync(input);

            await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrgUnitId, GetCurrentTenantId());
        }

        private async Task CheckExistenceOfRoleAndOrgUnitAsync(SetOrgUnitRoleDto input)
        {
            try
            {
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                var ou = await _orgUnitRepository.GetAsync(input.OrgUnitId);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }
        /// <summary>
        /// 对当前租户下的特定组织删除某一角色
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task RemoveRoleFromOrgUnitAsync(SetOrgUnitRoleDto input)
        {
            await CheckExistenceOfRoleAndOrgUnitAsync(input);

            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrgUnitId);
        }
        /// <summary>
        /// 获取当前租户下指定组织的全部角色
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public async Task<List<RoleDto>> GetRolesInOrgUnitAsync(EntityDto<long> input, bool includeChildren = false)
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
        /// <summary>
        /// 对当前租户下指定组织添加一个用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddUserToOrgUnitAsync(SetOrgUnitUserDto input)
        {
            await CheckExistenceOfUserAndOrgUnitAsync(input);

            await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrgUnitId);
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
        /// 删除当前租户下指定组织的某一用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task RemoveUserFromOrgUnitAsync(SetOrgUnitUserDto input)
        {
            await CheckExistenceOfUserAndOrgUnitAsync(input);

            await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrgUnitId);
        }
        /// <summary>
        /// 获取当前租户下指定组织的全部用户
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersInOrgUnitAsync(EntityDto<long> input, bool includeChildren = false)
        {
            var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

            if (orgUnit == null)
            {
                throw new UserFriendlyException(L("OrgUnitNotFound", input.Id));
            }

            var users = await _userManager.GetUsersInOrganizationUnitAsync(orgUnit, includeChildren);

            return ObjectMapper.Map<List<UserDto>>(users);
        }

        private int? GetCurrentTenantId()
        {
            if (CurrentUnitOfWork != null)
            {
                return CurrentUnitOfWork.GetTenantId();
            }

            return AbpSession.TenantId;
        }
    }
}
