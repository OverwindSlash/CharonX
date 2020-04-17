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
            CreateRoleDto createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
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
    }
}
