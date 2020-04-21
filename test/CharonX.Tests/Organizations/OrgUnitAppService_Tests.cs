using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Application.Services.Dto;
using CharonX.Organizations;
using CharonX.Organizations.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using CharonX.Authorization;
using CharonX.Authorization.Users;
using CharonX.Roles;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using Xunit;

namespace CharonX.Tests.Organizations
{
    public class OrgUnitAppService_Tests : CharonXTestBase
    {
        private readonly IOrgUnitAppService _orgUnitAppService;
        private readonly IRoleAppService _roleAppService;

        public OrgUnitAppService_Tests()
        {
            _orgUnitAppService = Resolve<IOrgUnitAppService>();
            _roleAppService = Resolve<IRoleAppService>();

            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task CreateOrgUnit_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.TenantId = 1;
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00001");
            });
        }

        [Fact]
        public async Task CreateOrgUnit_Hierarchy_Test()
        {
            CreateOrgUnitDto createParentOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Parent"
            };
            var parentOrgUnitDto = await _orgUnitAppService.CreateAsync(createParentOrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(
                    ou => ou.Id == parentOrgUnitDto.Id);
                testOu.TenantId = 1;
                testOu.DisplayName.ShouldBe("Ou Parent");
                testOu.Code.ShouldBe("00001");
            });

            CreateOrgUnitDto createChild1OrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = parentOrgUnitDto.Id,
                DisplayName = "Ou Child1"
            };
            var child1OrgUnitDto = await _orgUnitAppService.CreateAsync(createChild1OrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(
                    ou => ou.Id == child1OrgUnitDto.Id);
                testOu.TenantId = 1;
                testOu.DisplayName.ShouldBe("Ou Child1");
                testOu.Code.ShouldBe("00001.00001");
            });

            CreateOrgUnitDto createChild2OrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = parentOrgUnitDto.Id,
                DisplayName = "Ou Child2"
            };
            var child2OrgUnitDto = await _orgUnitAppService.CreateAsync(createChild2OrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(
                    ou => ou.Id == child2OrgUnitDto.Id);
                testOu.TenantId = 1;
                testOu.DisplayName.ShouldBe("Ou Child2");
                testOu.Code.ShouldBe("00001.00002");
            });
        }

        [Fact]
        public async Task GetOrgUnit_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            CreateOrgUnitDto createOrgUnitDto1 = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test1"
            };
            var orgUnitDto1 = await _orgUnitAppService.CreateAsync(createOrgUnitDto1);

            await UsingDbContextAsync(async context =>
            {
                int count = await context.OrganizationUnits.CountAsync();
                count.ShouldBe(2);
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00001");
            });

            var result = await _orgUnitAppService.GetAsync(new EntityDto<long>(orgUnitDto1.Id));
            result.DisplayName.ShouldBe("Ou Test1");
            result.Code.ShouldBe("00002");
        }

        [Fact]
        public async Task GetAllOrgUnit_Test()
        {
            int orgUnitCount = 50;

            for (int i = 0; i < orgUnitCount; i++)
            {
                CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
                {
                    ParentId = null,
                    DisplayName = $"Ou Test{i,2}"
                };
                var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);
            }

            PagedResultRequestDto pagedRoleDto = new PagedResultRequestDto()
            {
                SkipCount = 17,
                MaxResultCount = 8
            };

            var orgUnits = await _orgUnitAppService.GetAllAsync(pagedRoleDto);
            orgUnits.Items.Count.ShouldBe(8);
            orgUnits.Items[0].DisplayName.ShouldBe("Ou Test17");
        }

        [Fact]
        public async Task UpdateOrgUnit_ChangeDisplayName_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            var getOrgUnitDto = await _orgUnitAppService.GetAsync(new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.DisplayName = "**Ou Test**";

            var updatedOrgUnitDto = await _orgUnitAppService.UpdateAsync(getOrgUnitDto);
            updatedOrgUnitDto.DisplayName.ShouldBe("**Ou Test**");
            updatedOrgUnitDto.Code.ShouldBe("00001");
        }

        [Fact]
        public async Task UpdateOrgUnit_ChangeParentId_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            CreateOrgUnitDto createParentOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Parent"
            };
            var parentOrgUnitDto = await _orgUnitAppService.CreateAsync(createParentOrgUnitDto);

            var getOrgUnitDto = await _orgUnitAppService.GetAsync(new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.ParentId = parentOrgUnitDto.Id;
            getOrgUnitDto.DisplayName = "**Ou Test**";

            var updatedOrgUnitDto = await _orgUnitAppService.UpdateAsync(getOrgUnitDto);
            updatedOrgUnitDto.DisplayName.ShouldBe("**Ou Test**");
            updatedOrgUnitDto.Code.ShouldBe("00002.00001");
        }

        [Fact]
        public async Task UpdateOrgUnit_ChangeParentIdWithChild_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            CreateOrgUnitDto createChildOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = orgUnitDto.Id,
                DisplayName = "Ou Child"
            };
            var childOrgUnitDto = await _orgUnitAppService.CreateAsync(createChildOrgUnitDto);

            CreateOrgUnitDto createParentOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Parent"
            };
            var parentOrgUnitDto = await _orgUnitAppService.CreateAsync(createParentOrgUnitDto);

            var getOrgUnitDto = await _orgUnitAppService.GetAsync(new EntityDto<long>(orgUnitDto.Id));
            getOrgUnitDto.ParentId = parentOrgUnitDto.Id;
            getOrgUnitDto.DisplayName = "**Ou Test**";

            var updatedOrgUnitDto = await _orgUnitAppService.UpdateAsync(getOrgUnitDto);
            updatedOrgUnitDto.DisplayName.ShouldBe("**Ou Test**");
            updatedOrgUnitDto.Code.ShouldBe("00002.00001");

            var getChildOrgUnitDto = await _orgUnitAppService.GetAsync(new EntityDto<long>(childOrgUnitDto.Id));
            getChildOrgUnitDto.Code.ShouldBe("00002.00001.00001");
        }

        [Fact]
        public async Task DeleteOrgUnit_Exist_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.TenantId = 1;
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00001");
            });

            await _orgUnitAppService.DeleteAsync(new EntityDto<long>(orgUnitDto.Id));

            await UsingDbContextAsync(async context =>
            {
                var testOu = await context.OrganizationUnits.FirstOrDefaultAsync(ou => ou.Id == orgUnitDto.Id);
                testOu.IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task DeleteOrgUnit_NotExist_Test()
        {
            try
            {
                await _orgUnitAppService.DeleteAsync(new EntityDto<long>(9999));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("There is no such an entity. Entity type: Abp.Organizations.OrganizationUnit, id: 9999");
            }
        }

        [Fact]
        public async Task AddRoleToOrgUnit_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            SetOrgUnitRoleDto setOrgUnitRoleDto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = roleDto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRoleDto);

            await UsingDbContextAsync(async context =>
            {
                var testOus = await context.OrganizationUnitRoles
                    .Where(our => our.OrganizationUnitId == orgUnitDto.Id).ToListAsync();
                testOus.Count.ShouldBe(1);
                testOus[0].RoleId.ShouldBe(roleDto.Id);
            });
        }

        [Fact]
        public async Task RemoveRoleFromOrgUnit_Test()
        {
            var createRoleDto = new CreateRoleDto()
            {
                Name = "RoleTest",
                DisplayName = "Test role",
                Description = "Role for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var roleDto = await _roleAppService.CreateAsync(createRoleDto);

            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            SetOrgUnitRoleDto setOrgUnitRoleDto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = roleDto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRoleDto);

            await UsingDbContextAsync(async context =>
            {
                var testOus = await context.OrganizationUnitRoles
                    .Where(our => our.OrganizationUnitId == orgUnitDto.Id).ToListAsync();
                testOus.Count.ShouldBe(1);
                testOus[0].RoleId.ShouldBe(roleDto.Id);
            });

            await _orgUnitAppService.RemoveRoleFromOrgUnitAsync(setOrgUnitRoleDto);
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
        public async Task GetRolesInOrgUnit_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            var createRoleDto1 = new CreateRoleDto()
            {
                Name = "RoleTest1",
                DisplayName = "Test role1",
                Description = "Role1 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role1Dto = await _roleAppService.CreateAsync(createRoleDto1);

            SetOrgUnitRoleDto setOrgUnitRole1Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role1Dto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRole1Dto);

            var createRoleDto2 = new CreateRoleDto()
            {
                Name = "RoleTest2",
                DisplayName = "Test role2",
                Description = "Role2 for test",
                GrantedPermissions = new List<string>() { PermissionNames.Pages_Roles }
            };
            var role2Dto = await _roleAppService.CreateAsync(createRoleDto2);

            SetOrgUnitRoleDto setOrgUnitRole2Dto = new SetOrgUnitRoleDto()
            {
                OrgUnitId = orgUnitDto.Id,
                RoleId = role2Dto.Id
            };
            await _orgUnitAppService.AddRoleToOrgUnitAsync(setOrgUnitRole2Dto);

            var roles = await _orgUnitAppService.GetRolesInOrgUnitAsync(new EntityDto<long>(orgUnitDto.Id));
            roles.Count.ShouldBe(2);
            roles[0].DisplayName.ShouldBe("Test role1");
            roles[1].DisplayName.ShouldBe("Test role2");
        }

        [Fact]
        public async Task AddUserToOrgUnit_Test()
        {
            CreateOrgUnitDto createOrgUnitDto = new CreateOrgUnitDto()
            {
                ParentId = null,
                DisplayName = "Ou Test"
            };
            var orgUnitDto = await _orgUnitAppService.CreateAsync(createOrgUnitDto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Name = "John",
                Surname = "Smith",
                EmailAddress = "test@test.com",
                Password = User.DefaultPassword
            };
        }
    }
}
