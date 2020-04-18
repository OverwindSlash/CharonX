using CharonX.Authorization;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CharonX.Tests.Roles
{
    public class OmRoleAppService_Tests : CharonXTestBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IOmRoleAppService _omRoleAppService;

        public OmRoleAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omRoleAppService = Resolve<IOmRoleAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task CreateRoleInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            CreateRoleDto createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto);

            roleDto.Id.ShouldNotBe(0);
            roleDto.NormalizedName.ShouldBe("ROLETEST");

            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.TenantId.ShouldBe(tenantDto.Id);
            });
        }

        [Fact]
        public async Task GetAllRolesInTenant_Test()
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

            GetRolesInput input1 = new GetRolesInput()
            {
                Permission = string.Empty
            };
            var roles1 = await _omRoleAppService.GetAllRolesInTenantAsync(tenantDto.Id, input1);
            roles1.Items.Count.ShouldBe(3);

            GetRolesInput input2 = new GetRolesInput()
            {
                Permission = PermissionNames.Pages_Users
            };
            var roles2 = await _omRoleAppService.GetAllRolesInTenantAsync(tenantDto.Id, input2);
            roles2.Items.Count.ShouldBe(2);
        }
    }
}
