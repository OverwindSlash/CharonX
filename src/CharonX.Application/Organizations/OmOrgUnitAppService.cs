using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Organizations.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Abp.UI;
using CharonX.Authorization.Roles;
using CharonX.Roles.Dto;

namespace CharonX.Organizations
{
    public class OmOrgUnitAppService : ApplicationService, IOmOrgUnitAppService
    {
        private readonly OrganizationUnitManager _orgUnitManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly RoleManager _roleManager;

        public OmOrgUnitAppService(
            OrganizationUnitManager orgUnitManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            RoleManager roleManager)
        {
            _orgUnitManager = orgUnitManager;
            _orgUnitRepository = orgUnitRepository;
            _roleManager = roleManager;
        }

        public async Task<OrgUnitDto> CreateOrgUnitInTenantAsync(int tenantId, CreateOrgUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = ObjectMapper.Map<OrganizationUnit>(input);
                orgUnit.TenantId = tenantId;

                await _orgUnitManager.CreateAsync(orgUnit);
                await CurrentUnitOfWork.SaveChangesAsync();

                return await GenerateOrgUnitDtoAsync(orgUnit);
            }
        }

        private async Task<OrgUnitDto> GenerateOrgUnitDtoAsync(OrganizationUnit orgUnit)
        {
            var orgUnitDto = ObjectMapper.Map<OrgUnitDto>(orgUnit);
            var roles = await _roleManager.GetRolesInOrganizationUnit(orgUnit);
            orgUnitDto.AssignedRoles = roles.Select(r => r.Name).ToList();

            return orgUnitDto;
        }

        public async Task<OrgUnitDto> GetOrgUnitInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

                return await GenerateOrgUnitDtoAsync(orgUnit);
            }
        }

        public Task<ListResultDto<OrgUnitListDto>> GetAllOrgUnitInTenantAsync(int tenantId, GetOrgUnitsInput input)
        {
            throw new NotImplementedException();
        }

        public Task<OrgUnitDto> UpdateOrgUnitInTenantAsync(int tenantId, OrgUnitDto input)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOrgUnitInTenantAsync(int tenantId, EntityDto<long> input)
        {
            throw new NotImplementedException();
        }

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

                return ObjectMapper.Map<List<RoleDto>>(roles);
            }
        }
    }
}
