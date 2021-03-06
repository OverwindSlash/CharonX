﻿using Abp.Application.Services.Dto;
using CharonX.Authorization;
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
    public class RoleAppService_Tests : CharonXTestBase
    {
        private readonly IRoleAppService _roleAppService;
        private readonly IUserAppService _userAppService;

        public RoleAppService_Tests()
        {
            _roleAppService = Resolve<IRoleAppService>();
            _userAppService = Resolve<IUserAppService>();

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
        public async Task GetRolesByPermission_Test()
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
            var roles1 = await _roleAppService.GetRolesByPermissionAsync(input1);
            roles1.Items.Count.ShouldBe(3);     // + Admin Role

            GetRolesInput input2 = new GetRolesInput()
            {
                Permission = PermissionNames.Pages_Users
            };
            var roles2 = await _roleAppService.GetRolesByPermissionAsync(input2);
            roles2.Items.Count.ShouldBe(2);     // Admin & Role2
        }


        [Fact]
        public async Task GetAllRoles_Test()
        {
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
                var roleDto = await _roleAppService.CreateAsync(createRoleDto);
            }

            PagedRoleResultRequestDto pagedRoleDto = new PagedRoleResultRequestDto()
            {
                Keyword = string.Empty,
                SkipCount = 17,
                MaxResultCount = 8
            };

            var roles = await _roleAppService.GetAllAsync(pagedRoleDto);
            roles.Items.Count.ShouldBe(8);
            roles.Items[0].Name.ShouldBe("Role16");
        }


        [Fact]
        public async Task GetRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };

            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            var getRoleDto = await _roleAppService.GetAsync(new EntityDto<int>(roleDto.Id));
            getRoleDto.Name.ShouldBe(createRoleDto.Name);
            getRoleDto.DisplayName.ShouldBe(createRoleDto.DisplayName);
            getRoleDto.Description.ShouldBe(createRoleDto.Description);
            getRoleDto.GrantedPermissions.Count.ShouldBe(1);
        }

        [Fact]
        public async Task UpdateRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };

            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            roleDto.DisplayName = "**Test role**";
            roleDto.Description = "**Role for test**";
            roleDto.GrantedPermissions = new List<string>() { PermissionNames.Pages_Users };

            var updateRoleDto = await _roleAppService.UpdateAsync(roleDto);

            updateRoleDto.DisplayName.ShouldBe(roleDto.DisplayName);
            updateRoleDto.Description.ShouldBe(roleDto.Description);
            updateRoleDto.GrantedPermissions.Count.ShouldBe(1);
            updateRoleDto.GrantedPermissions[0].ShouldBe(PermissionNames.Pages_Users);
        }

        [Fact]
        public async Task DeleteRole_Exist_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.ShouldNotBeNull();
            });

            await _roleAppService.DeleteAsync(new EntityDto<int>(roleDto.Id));
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.Id);
                testTenant.IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task DeleteRole_NotExist_Test()
        {
            try
            {
                await _roleAppService.DeleteAsync(new EntityDto<int>(9999));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("Role 9999 not found.");
            }
        }

        [Fact]
        public async Task AddUserToRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                IsActive = true
            };
            var userDto = await _userAppService.CreateAsync(createUserDto);

            var getUser1Dto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUser1Dto.RoleNames.Length.ShouldBe(0);

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = userDto.Id,
                RoleId = roleDto.Id
            };

            await _roleAppService.AddUserToRoleAsync(setRoleUserDto);

            var getUser2Dto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUser2Dto.RoleNames.Length.ShouldBe(1);
        }

        [Fact]
        public async Task RemoveUserFromRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400000",
                IsActive = true
            };
            var userDto = await _userAppService.CreateAsync(createUserDto);

            var getUser1Dto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUser1Dto.RoleNames.Length.ShouldBe(0);

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = userDto.Id,
                RoleId = roleDto.Id
            };
            await _roleAppService.AddUserToRoleAsync(setRoleUserDto);

            var getUser2Dto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUser2Dto.RoleNames.Length.ShouldBe(1);


            await _roleAppService.RemoveUserFromRoleAsync(setRoleUserDto);
            var getUser3Dto = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
            getUser3Dto.RoleNames.Length.ShouldBe(0);
        }

        [Fact]
        public async Task GetUsersInRole_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            CreateUserDto createUser1Dto = new CreateUserDto()
            {
                UserName = "TestUser1",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                PhoneNumber = "13851400001",
                IsActive = true
            };
            var user1Dto = await _userAppService.CreateAsync(createUser1Dto);

            SetRoleUserDto setRoleUserDto = new SetRoleUserDto()
            {
                UserId = user1Dto.Id,
                RoleId = roleDto.Id
            };
            await _roleAppService.AddUserToRoleAsync(setRoleUserDto);

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
            var user2Dto = await _userAppService.CreateAsync(createUser2Dto);

            var roleUsers = await _roleAppService.GetUsersInRoleAsync(new EntityDto<int>(roleDto.Id));
            roleUsers.Count.ShouldBe(2);
        }
    }
}