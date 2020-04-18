using CharonX.Authorization;
using CharonX.Roles;
using CharonX.Roles.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CharonX.Tests.Roles
{
    public class RoleAppService_Tests : CharonXTestBase
    {
        private readonly IRoleAppService _roleAppService;

        public RoleAppService_Tests()
        {
            _roleAppService = Resolve<IRoleAppService>();

            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task CreateRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };

            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            roleDto.Id.ShouldNotBe(0);
            roleDto.NormalizedName.ShouldBe("ROLETEST");

            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.TenantId.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetRoles_Test()
        {
            var createRoleDto1 = new CreateRoleDto()
            {
                Name = "Role1",
                DisplayName = "Test role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };
            var role1Dto = await _roleAppService.CreateAsync(createRoleDto1);

            var createRoleDto2 = new CreateRoleDto()
            {
                Name = "Role2",
                DisplayName = "Test role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Users}
            };
            var role2Dto = await _roleAppService.CreateAsync(createRoleDto2);

            GetRolesInput input1 = new GetRolesInput()
            {
                Permission = string.Empty
            };
            var roles1 = await _roleAppService.GetRolesAsync(input1);
            roles1.Items.Count.ShouldBe(3);     // + Admin Role

            GetRolesInput input2 = new GetRolesInput()
            {
                Permission = PermissionNames.Pages_Users
            };
            var roles2 = await _roleAppService.GetRolesAsync(input2);
            roles2.Items.Count.ShouldBe(2);     // Admin & Role2
        }
    }
}