using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CharonX.Authorization;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Organizations;
using CharonX.Organizations.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CharonX.Tests.Organizations
{
    public class OmOrgUnitAppService_Tests : CharonXTestBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IOmRoleAppService _omRoleAppService;
        private readonly IOmOrgUnitAppService _omOrgUnitAppService;

        public OmOrgUnitAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omRoleAppService = Resolve<IOmRoleAppService>();
            _omOrgUnitAppService = Resolve<IOmOrgUnitAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task CreateOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.TenantId = tenantDto.Id;
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00002");  // AdminGroup is 00001
            });
        }

        [Fact]
        public async Task CreateOrgUnitInTenant_WithRole_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            var createRoleDto1 = new CreateRoleDto()
            {
                Name = "Role1",
                DisplayName = "Test role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role1Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto1);

            var createRoleDto2 = new CreateRoleDto()
            {
                Name = "Role2",
                DisplayName = "Test role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Users }
            };
            var role2Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto2);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.TenantId = tenantDto.Id;
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00002");  // AdminGroup is 00001
            });
        }

        [Fact]
        public async Task GetOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            var getOrgUnitDto =
                await _omOrgUnitAppService.GetOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.DisplayName.ShouldBe("Ou Test");
            getOrgUnitDto.Code.ShouldBe("00002");  // AdminGroup is 00001
        }


    }
}
