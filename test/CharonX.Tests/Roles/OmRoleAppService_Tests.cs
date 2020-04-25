using Abp.Application.Services.Dto;
using CharonX.Authorization;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharonX.Authorization.Users;
using CharonX.Users;
using CharonX.Users.Dto;
using Xunit;

namespace CharonX.Tests.Roles
{
    public class OmRoleAppService_Tests : CharonXTestBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IOmRoleAppService _omRoleAppService;
        private readonly IOmUserAppService _omUserAppService;

        public OmRoleAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omRoleAppService = Resolve<IOmRoleAppService>();
            _omUserAppService = Resolve<IOmUserAppService>();

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
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
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
        public async Task GetRolesByPermissionInTenant_Test()
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
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };
            var role1Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto1);

            var createRoleDto2 = new CreateRoleDto()
            {
                Name = "Role2",
                DisplayName = "Test role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Users}
            };
            var role2Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto2);

            GetRolesInput input1 = new GetRolesInput()
            {
                Permission = string.Empty
            };
            var roles1 = await _omRoleAppService.GetRolesByPermissionInTenantAsync(tenantDto.Id, input1);
            roles1.Items.Count.ShouldBe(3);

            GetRolesInput input2 = new GetRolesInput()
            {
                Permission = PermissionNames.Pages_Users
            };
            var roles2 = await _omRoleAppService.GetRolesByPermissionInTenantAsync(tenantDto.Id, input2);
            roles2.Items.Count.ShouldBe(2);
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

            int roleCount = 50;

            for (int i = 0; i < roleCount; i++)
            {
                var createRoleDto = new CreateRoleDto()
                {
                    Name = $"Role{i,2}",
                    DisplayName = $"Test role{i,2}",
                    Description = $"Role{i,2} for test",
                    GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles, PermissionNames.Pages_Users }
                };
                var roleDto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto);
            }

            PagedRoleResultRequestDto pagedRoleDto = new PagedRoleResultRequestDto()
            {
                Keyword = string.Empty,
                SkipCount = 13,
                MaxResultCount = 6
            };

            var roles = await _omRoleAppService.GetAllRolesInTenantAsync(tenantDto.Id, pagedRoleDto);
            roles.Items.Count.ShouldBe(6);
            roles.Items[0].Name.ShouldBe("Role12");
        }

        [Fact]
        public async Task GetRoleInTenant_CorrectTenantId_Test()
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
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };
            var role1Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto1);

            var getRole1Dto =
                await _omRoleAppService.GetRoleInTenantAsync(tenantDto.Id, new EntityDto<int>(role1Dto.Id));
            getRole1Dto.Name.ShouldBe(createRoleDto1.Name);
            getRole1Dto.DisplayName.ShouldBe(createRoleDto1.DisplayName);
            getRole1Dto.Description.ShouldBe(createRoleDto1.Description);
            getRole1Dto.GrantedPermissions.Count.ShouldBe(1);
        }

        [Fact]
        public async Task GetRoleInTenant_WrongTenantId_Test()
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
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };
            var role1Dto = await _omRoleAppService.CreateRoleInTenantAsync(tenantDto.Id, createRoleDto1);

            try
            {
                var getRole1Dto = await _omRoleAppService.GetRoleInTenantAsync(1, new EntityDto<int>(role1Dto.Id));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("There is no role with id: 4");
            }
        }

        [Fact]
        public async Task UpdateRoleInTenant_CorrectTenantId_Test()
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

            roleDto.DisplayName = "**Test role**";
            roleDto.Description = "**Role for test**";
            roleDto.GrantedPermissions = new List<string>() { PermissionNames.Pages_Users };

            var updateRoleDto = await _omRoleAppService.UpdateRoleInTenantAsync(tenantDto.Id, roleDto);

            updateRoleDto.DisplayName.ShouldBe(roleDto.DisplayName);
            updateRoleDto.Description.ShouldBe(roleDto.Description);
            updateRoleDto.GrantedPermissions.Count.ShouldBe(1);
            updateRoleDto.GrantedPermissions[0].ShouldBe(PermissionNames.Pages_Users);
        }

        [Fact]
        public async Task UpdateRoleInTenant_WrongTenantId_Test()
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

            roleDto.DisplayName = "**Test role**";
            roleDto.Description = "**Role for test**";
            roleDto.GrantedPermissions = new List<string>() { PermissionNames.Pages_Users };

            try
            {
                var updateRoleDto = await _omRoleAppService.UpdateRoleInTenantAsync(1, roleDto);
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("There is no role with id: 4");
            }
        }

        [Fact]
        public async Task DeleteRoleInTenant_Exist_Test()
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
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.ShouldNotBeNull();
            });

            await _omRoleAppService.DeleteRoleInTenantAsync(tenantDto.Id, new EntityDto<int>(roleDto.Id));
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task DeleteRoleInTenant_NotExist_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            try
            {
                await _omRoleAppService.DeleteRoleInTenantAsync(tenantDto.Id, new EntityDto<int>(9999));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("Role 9999 not found.");
            }
        }

        [Fact]
        public async Task DeleteRoleInTenant_WrongTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            try
            {
                await _omRoleAppService.DeleteRoleInTenantAsync(99, new EntityDto<int>(9999));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("There is no tenant with given id: 99");
            }
        }

        [Fact]
        public async Task AddUserToRoleInTenant_Test()
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
            getUser1Dto.RoleNames.Length.ShouldBe(0);

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = userDto.Id,
                RoleId = roleDto.Id
            };
            await _omRoleAppService.AddUserToRoleInTenantAsync(tenantDto.Id, setRoleUserDto);

            var getUser2Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser2Dto.RoleNames.Length.ShouldBe(1);
        }

        [Fact]
        public async Task RemoveUserFromRoleInTenant_Test()
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
            getUser1Dto.RoleNames.Length.ShouldBe(0);

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = userDto.Id,
                RoleId = roleDto.Id
            };
            await _omRoleAppService.AddUserToRoleInTenantAsync(tenantDto.Id, setRoleUserDto);

            var getUser2Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id,new EntityDto<long>(userDto.Id));
            getUser2Dto.RoleNames.Length.ShouldBe(1);


            await _omRoleAppService.RemoveUserFromRoleInTenantAsync(tenantDto.Id, setRoleUserDto);
            var getUser3Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser3Dto.RoleNames.Length.ShouldBe(0);
        }

        [Fact]
        public async Task GetUsersInRoleInTenant_Test()
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

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = user1Dto.Id,
                RoleId = roleDto.Id
            };
            await _omRoleAppService.AddUserToRoleInTenantAsync(tenantDto.Id, setRoleUserDto);

            CreateUserDto createUser2Dto = new CreateUserDto()
            {
                UserName = "TestUser2",
                Password = User.DefaultPassword,
                Name = "Mike",
                Surname = "Smith",
                PhoneNumber = "13851400002",
                IsActive = true,
                RoleNames = new[] { "RoleTest" }
            };
            var user2Dto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUser2Dto);

            var roleUsers = await _omRoleAppService.GetUsersInRoleInTenantAsync(tenantDto.Id, new EntityDto<int>(roleDto.Id));
            roleUsers.Count.ShouldBe(2);
        }
    }
}
