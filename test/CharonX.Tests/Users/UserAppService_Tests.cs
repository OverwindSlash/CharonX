using CharonX.Authorization.Users;
using CharonX.Users;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharonX.Authorization;
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
        public async Task CreateUser_WithRoleAndOrgUnit_Test()
        {
            // Role 1
            var createRole1Dto = new CreateRoleDto()
            {
                Name = "Role1",
                DisplayName = "Role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role1Dto = await _roleAppService.CreateAsync(createRole1Dto);

            // Role 2
            var createRole2Dto = new CreateRoleDto()
            {
                Name = "Role2",
                DisplayName = "Role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Users, PermissionNames.Pages_Tenants }
            };
            var role2Dto = await _roleAppService.CreateAsync(createRole2Dto);

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

            // Create user with role and orgunit (orgunit contain role)
            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                RoleNames = new [] { "Role1" },
                OrgUnitNames = new [] { "Ou Test" }
            };

            var userDto = await _userAppService.CreateAsync(createUserDto);

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users
                    .Include(u => u.Roles)
                    .Include(u => u.Permissions)
                    .FirstOrDefaultAsync(u => u.Id == userDto.Id);
                user.Roles.Count.ShouldBe(2);
                user.Permissions.Count.ShouldBe(3);
                var permissions = user.Permissions.ToList();
            });
        }
    }
}
