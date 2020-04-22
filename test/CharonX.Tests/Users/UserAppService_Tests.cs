using CharonX.Authorization.Users;
using CharonX.Users;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Organizations;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Organizations;
using CharonX.Organizations.Dto;
using CharonX.Roles;
using CharonX.Roles.Dto;
using Xunit;

namespace CharonX.Tests.Users
{
    public class UserAppService_Tests : CharonXTestBase
    {
        private readonly IUserAppService _userAppService;
        private readonly IRoleAppService _roleAppService;
        private readonly IOrgUnitAppService _orgUnitAppService;

        public UserAppService_Tests()
        {
            _userAppService = Resolve<IUserAppService>();
            _roleAppService = Resolve<IRoleAppService>();
            _orgUnitAppService = Resolve<IOrgUnitAppService>();
        }

        [Fact]
        public async Task GetUsers_Test()
        {
            // Act
            var output = await _userAppService.GetAllAsync(new PagedUserResultRequestDto{MaxResultCount=20, SkipCount=0} );

            // Assert
            output.Items.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task CreateUser_NoRoleNoOrgUnit_Test()
        {
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                Gender = "M",
                IdNumber = "320101198001010021",
                PhoneNumber = "13851400000",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _userAppService.CreateAsync(createUserDto);

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);
                user.FullName.ShouldBe("John Smith");
            });
        }

        [Fact]
        public async Task CreateUser_DuplicatedPhone_Test()
        {
            CreateUserDto createUser1Dto = new CreateUserDto()
            {
                UserName = "TestUser1",
                Password = User.DefaultPassword,
                Name = "Test",
                Surname = "User",
                PhoneNumber = "13851400000"
            };
            var user1Dto = await _userAppService.CreateAsync(createUser1Dto);

            CreateUserDto createUser2Dto = new CreateUserDto()
            {
                UserName = "TestUser2",
                Password = User.DefaultPassword,
                Name = "Test",
                Surname = "User",
                PhoneNumber = "13851400000"
            };
            try
            {
                var user2Dto = await _userAppService.CreateAsync(createUser2Dto);
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("Phone number " + createUser2Dto.PhoneNumber + " duplicated.");
            }
        }

        [Fact]
        public async Task CreateUser_WithRoleAndOrgUnit_Test()
        {
            await CreateComplexRoleAndOrgUnit();

            // Create user with role and orgunit (orgunit contain role)
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                RoleNames = new[] { "Role1" },
                OrgUnitNames = new[] { "Ou Test" }
            };

            var userDto = await _userAppService.CreateAsync(createUserDto);

            userDto.FullName.ShouldBe("John Smith");
            userDto.OrgUnitNames.Length.ShouldBe(1);
            userDto.RoleNames.Length.ShouldBe(2);
            userDto.Permissions.Length.ShouldBe(2);
        }

        private async Task CreateComplexRoleAndOrgUnit()
        {
            // Role 1
            var createRole1Dto = new CreateRoleDto()
            {
                Name = "Role1",
                DisplayName = "Role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Roles}
            };
            var role1Dto = await _roleAppService.CreateAsync(createRole1Dto);

            // Role 2
            var createRole2Dto = new CreateRoleDto()
            {
                Name = "Role2",
                DisplayName = "Role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() {PermissionNames.Pages_Users, PermissionNames.Pages_Roles}
            };
            var role2Dto = await _roleAppService.CreateAsync(createRole2Dto);

            // Role 2
            var createRole3Dto = new CreateRoleDto()
            {
                Name = "Role3",
                DisplayName = "Role3",
                Description = "Role3 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role3Dto = await _roleAppService.CreateAsync(createRole3Dto);

            // OrgUnit with Role1 and Role2
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            SetOrgUnitRoleDto setOrgUnitRole1Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role1Dto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRole1Dto);

            SetOrgUnitRoleDto setOrgUnitRole2Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role2Dto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRole2Dto);
        }

        [Fact]
        public async Task GetUser_Test()
        {
            await CreateComplexRoleAndOrgUnit();

            // Create user with role and orgunit (orgunit contain role)
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                RoleNames = new[] { "Role1" },
                OrgUnitNames = new[] { "Ou Test" }
            };

            var userDto = await _userAppService.CreateAsync(createUserDto);

            var getUserDto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUserDto.FullName.ShouldBe("John Smith");
            getUserDto.OrgUnitNames.Length.ShouldBe(1);
            getUserDto.RoleNames.Length.ShouldBe(2);
            getUserDto.Permissions.Length.ShouldBe(2);
        }

        [Fact]
        public async Task GetAllUsers_Test()
        {
            await CreateComplexRoleAndOrgUnit();

            int userCount = 50;
            for (int i = 10; i < userCount; i++)
            {
                CreateUserDto createUserDto = new CreateUserDto()
                {
                    UserName = $"TestUser{i,2}",
                    Password = User.DefaultPassword,
                    Name = $"Test{i,2}",
                    Surname = $"User{i,2}",
                    PhoneNumber = $"138514000{i,2}",
                    RoleNames = new[] { "Role1" },
                    OrgUnitNames = new[] { "Ou Test" }
                };
                var userDto = await _userAppService.CreateAsync(createUserDto);
            }

            PagedUserResultRequestDto input = new PagedUserResultRequestDto()
            {
                IsActive = false,
                Keyword = string.Empty,
                SkipCount = 17,
                MaxResultCount = 6
            };

            var users = await _userAppService.GetAllAsync(input);
            users.Items.Count.ShouldBe(6);
            users.Items[0].UserName.ShouldBe("TestUser27");
            users.Items[0].OrgUnitNames.Length.ShouldBe(1);
            users.Items[0].RoleNames.Length.ShouldBe(2);
            users.Items[0].Permissions.Length.ShouldBe(2);
        }

        [Fact]
        public async Task UpdateUsers_Test()
        {
            await CreateComplexRoleAndOrgUnit();

            // Create user with role and orgunit (orgunit contain role)
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                RoleNames = new[] { "Role1" },
                OrgUnitNames = new[] { "Ou Test" }
            };

            var userDto = await _userAppService.CreateAsync(createUserDto);

            var getUserDto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUserDto.Name = "Johnny";
            getUserDto.RoleNames = new string[] { "Role3" };

            var updatedUser = await _userAppService.UpdateAsync(getUserDto);
            updatedUser.FullName.ShouldBe("Johnny Smith");
            updatedUser.OrgUnitNames.Length.ShouldBe(1);
            updatedUser.RoleNames.Length.ShouldBe(3);
            updatedUser.Permissions.Length.ShouldBe(2);
        }

        [Fact]
        public async Task DeleteUser_Test()
        {
            await CreateComplexRoleAndOrgUnit();

            // Create user with role and orgunit (orgunit contain role)
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                RoleNames = new[] { "Role1" },
                OrgUnitNames = new[] { "Ou Test" }
            };

            UserManager userManager = Resolve<UserManager>();
            OrganizationUnitManager orgUnitManager = Resolve<OrganizationUnitManager>();

            var userDto = await _userAppService.CreateAsync(createUserDto);

            var usersInRoleBefore = (List<User>)await userManager.GetUsersInRoleAsync("Role1");
            usersInRoleBefore.Count.ShouldBe(1);
            
            await _userAppService.DeleteAsync(new EntityDto<long>(userDto.Id));

            var usersInRoleAfter = (List<User>)await userManager.GetUsersInRoleAsync("Role1");
            usersInRoleAfter.Count.ShouldBe(0);

        }
    }
}
