using CharonX.Authorization;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Collections.Generic;
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
    }
}
