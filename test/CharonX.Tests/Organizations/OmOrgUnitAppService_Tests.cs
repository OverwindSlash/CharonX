using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using CharonX.Authorization;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Organizations;
using CharonX.Organizations.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using CharonX.Users;
using CharonX.Users.Dto;
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
        private readonly IOmUserAppService _omUserAppService;

        public OmOrgUnitAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omRoleAppService = Resolve<IOmRoleAppService>();
            _omOrgUnitAppService = Resolve<IOmOrgUnitAppService>();
            _omUserAppService = Resolve<IOmUserAppService>();

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

            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role1Dto.Id
            });

            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role2Dto.Id
            });

            var getOrgUnitDto = await _omOrgUnitAppService.GetOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.AssignedRoles.Count.ShouldBe(2);
            getOrgUnitDto.GrantedPermissions.Count.ShouldBe(2);

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

        [Fact]
        public async Task GetAllOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            CreateOrgUnitDto createOrgUnit1Dto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou1"
            };
            var orgUnit1Dto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnit1Dto);

            CreateOrgUnitDto createOrgUnit2Dto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou2"
            };
            var orgUnit2Dto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnit2Dto);

            GetOrgUnitsInput getOrgUnitsInput = new GetOrgUnitsInput()
            {
                Role = string.Empty
            };
             var orgUnits = await _omOrgUnitAppService.GetAllOrgUnitInTenantAsync(tenantDto.Id, getOrgUnitsInput);
            orgUnits.Items.Count.ShouldBe(3);  // 2 new created + 1 admin 
        }

        [Fact]
        public async Task GetAllOrgUnitInTenant_FilterWithRole_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            // Prepare roles
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

            // Prepare organization units
            CreateOrgUnitDto createOrgUnit1Dto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou1"
            };
            var orgUnit1Dto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnit1Dto);

            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnit1Dto.Id,
                RoleId = role1Dto.Id
            });

            CreateOrgUnitDto createOrgUnit2Dto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou2"
            };
            var orgUnit2Dto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnit2Dto);

            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnit2Dto.Id,
                RoleId = role2Dto.Id
            });

            // Query
            GetOrgUnitsInput getOrgUnitsInput = new GetOrgUnitsInput()
            {
                Role = "Role2"
            };
            var orgUnits = await _omOrgUnitAppService.GetAllOrgUnitInTenantAsync(tenantDto.Id, getOrgUnitsInput);
            orgUnits.Items.Count.ShouldBe(2);  // 1 new created + 1 admin 
        }

        [Fact]
        public async Task UpdateOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            // Prepare roles
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

            // Prepare organization units
            CreateOrgUnitDto createOrgUnit1Dto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou1"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnit1Dto);

            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role1Dto.Id
            });

            // Update
            orgUnitDto.DisplayName = "**Ou1**";
            var updatedOrgUnitDto = await _omOrgUnitAppService.UpdateOrgUnitInTenantAsync(tenantDto.Id, orgUnitDto);
            updatedOrgUnitDto.DisplayName.ShouldBe("**Ou1**");
        }

        [Fact]
        public async Task DeleteOrgUnitInTenant_Test()
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
                DisplayName = "Ou1"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            await _omOrgUnitAppService.DeleteOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.TenantId.ShouldBe(tenantDto.Id);
                testOu.IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task AddRoleToOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            SetOrgUnitRoleDto setOrgUnitRoleDto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = roleDto.Id
            };
            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitRoleDto);

            await UsingDbContextAsync(async context =>
            {
                var testOus = await context.OrganizationUnitRoles
                    .Where(our => our.OrganizationUnitId == orgUnitDto.Id).ToListAsync();
                testOus.Count.ShouldBe(1);
                testOus[0].RoleId.ShouldBe(roleDto.Id);
            });
        }

        [Fact]
        public async Task RemoveRoleFromOrgUnitInTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _omOrgUnitAppService.CreateOrgUnitInTenantAsync(tenantDto.Id, createOrgUnitDto);

            SetOrgUnitRoleDto setOrgUnitRoleDto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = roleDto.Id
            };
            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitRoleDto);

            await UsingDbContextAsync(async context =>
            {
                var testOus = await context.OrganizationUnitRoles
                    .Where(our => our.OrganizationUnitId == orgUnitDto.Id).ToListAsync();
                testOus.Count.ShouldBe(1);
                testOus[0].RoleId.ShouldBe(roleDto.Id);
            });

            await _omOrgUnitAppService.RemoveRoleFromOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitRoleDto);
            await UsingDbContextAsync(async context =>
            {
                var testOus = await context.OrganizationUnitRoles
                    .Where(our => our.OrganizationUnitId == orgUnitDto.Id).ToListAsync();
                testOus.Count.ShouldBe(1);
                testOus[0].RoleId.ShouldBe(roleDto.Id);
                testOus[0].IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task GetRolesInOrgUnitInTenant_Test()
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

            var createRoleDto1 = new CreateRoleDto()
            {
                Name = "RoleTest1",
                DisplayName = "Test role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role1Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto1);

            SetOrgUnitRoleDto setOrgUnitRole1Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role1Dto.Id
            };
            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitRole1Dto);

            var createRoleDto2 = new CreateRoleDto()
            {
                Name = "RoleTest2",
                DisplayName = "Test role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Users }
            };
            var role2Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto2);

            SetOrgUnitRoleDto setOrgUnitRole2Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role2Dto.Id
            };
            await _omOrgUnitAppService.AddRoleToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitRole2Dto);

            var getOrgUnitDto = await _omOrgUnitAppService.GetOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.AssignedRoles.Count.ShouldBe(2);
            getOrgUnitDto.GrantedPermissions.Count.ShouldBe(2);

            var roles = await _omOrgUnitAppService.GetRolesInOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));
            roles.Count.ShouldBe(2);
            roles[0].DisplayName.ShouldBe("Test role1");
            roles[0].GrantedPermissions.Count.ShouldBe(1);
            roles[1].DisplayName.ShouldBe("Test role2");
            roles[1].GrantedPermissions.Count.ShouldBe(1);
        }

        [Fact]
        public async Task AddUserToOrgUnitInTenant_Test()
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

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400001",
                IsActive = true
            };
            var userDto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUserDto);

            var getUser1Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser1Dto.OrgUnitNames.Length.ShouldBe(0);

            SetOrgUnitUserDto setOrgUnitUserDto = new SetOrgUnitUserDto()
            {
                UserId = userDto.Id,
                OrgUnitId = orgUnitDto.Id
            };
            await _omOrgUnitAppService.AddUserToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitUserDto);

            var getUser2Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser2Dto.OrgUnitNames.Length.ShouldBe(1);
        }

        [Fact]
        public async Task RemoveUserFromOrgUnitInTenant_Test()
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

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400001",
                IsActive = true
            };
            var userDto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUserDto);

            var getUser1Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser1Dto.OrgUnitNames.Length.ShouldBe(0);

            SetOrgUnitUserDto setOrgUnitUserDto = new SetOrgUnitUserDto()
            {
                UserId = userDto.Id,
                OrgUnitId = orgUnitDto.Id
            };
            await _omOrgUnitAppService.AddUserToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitUserDto);

            var getUser2Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser2Dto.OrgUnitNames.Length.ShouldBe(1);

            // Remove from organization unit
            await _omOrgUnitAppService.RemoveUserFromOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitUserDto);
            var getUser3Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser3Dto.OrgUnitNames.Length.ShouldBe(0);
        }

        [Fact]
        public async Task GetUsersInOrgUnitInTenant_Test()
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

            CreateUserDto createUser1Dto = new CreateUserDto()
            {
                UserName = "TestUser1",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400001",
                IsActive = true
            };
            var user1Dto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUser1Dto);

            SetOrgUnitUserDto setOrgUnitUserDto = new SetOrgUnitUserDto()
            {
                UserId = user1Dto.Id,
                OrgUnitId = orgUnitDto.Id
            };
            await _omOrgUnitAppService.AddUserToOrgUnitInTenantAsync(tenantDto.Id, setOrgUnitUserDto);

            CreateUserDto createUser2Dto = new CreateUserDto()
            {
                UserName = "TestUser2",
                Password = User.DefaultPassword,
                Name = "Mike",
                Surname = "Smith",
                PhoneNumber = "13851400002",
                IsActive = true,
                OrgUnitNames = new[] { "Ou Test" }
            };
            var user2Dto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUser2Dto);

            var orgUnitUsers = await _omOrgUnitAppService.GetUsersInOrgUnitInTenantAsync(tenantDto.Id, new EntityDto<long>(orgUnitDto.Id));
            orgUnitUsers.Count.ShouldBe(2);
        }
    }
}
