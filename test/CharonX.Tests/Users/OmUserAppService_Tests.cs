using Abp.Application.Services.Dto;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Sessions;
using CharonX.Users;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CharonX.Tests.Users
{
    public class OmUserAppService_Tests : CharonXTestBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IOmUserAppService _omUserAppService;
        private readonly IUserAppService _userAppService;
        private readonly ISessionAppService _sessionAppService; 

        public OmUserAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omUserAppService = Resolve<IOmUserAppService>();
            _userAppService = Resolve<IUserAppService>();
            _sessionAppService = Resolve<ISessionAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task CreateUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                Gender = "M",
                IdNumber = "320101198001010021",
                PhoneNumber = "13851400001",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUserDto);

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.TenantId == tenantDto.Id);
                user.FullName.ShouldBe("John Smith");
            });

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.TenantId == 1);
                user.ShouldBeNull();
            });
        }

        [Fact]
        public async Task GetUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                Gender = "M",
                IdNumber = "320101198001010021",
                PhoneNumber = "13851400001",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUserDto);

            var getUser1Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser1Dto.FullName.ShouldBe("John Smith");
            getUser1Dto.IsAdmin.ShouldBeFalse();

            try
            {
                var getUser2Dto = await _omUserAppService.GetUserInTenantAsync(1, new EntityDto<long>(userDto.Id));
            }
            catch (Exception exception)
            {
                exception.Message.ShouldBe("User 4 not found.");
            }           
        }

        [Fact]
        public async Task CreateAdminUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                Gender = "M",
                IdNumber = "320101198001010021",
                PhoneNumber = "13851400001",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _omUserAppService.CreateAdminUserInTenantAsync(tenantDto.Id, createUserDto);

            var getUser1Dto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUser1Dto.FullName.ShouldBe("John Smith");
            getUser1Dto.IsAdmin.ShouldBeTrue();
            getUser1Dto.OrgUnitNames[0].ShouldBe("AdminGroup");
            getUser1Dto.RoleNames[0].ShouldBe("Admin");
        }

        [Fact]
        public async Task GetAllAdminUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            CreateUserDto createUserDto = new CreateUserDto()
            {
                UserName = "TestUser",
                Password = User.DefaultPassword,
                Name = "John",
                Surname = "Smith",
                Gender = "M",
                IdNumber = "320101198001010021",
                PhoneNumber = "13851400001",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _omUserAppService.CreateAdminUserInTenantAsync(tenantDto.Id, createUserDto);

            var allAdmins = await _omUserAppService.GetAllAdminUserInTenantAsync(tenantDto.Id);
            allAdmins.Count.ShouldBe(2);
            allAdmins[0].UserName.ShouldBe("admin");
            allAdmins[1].UserName.ShouldBe("TestUser");
        }

        [Fact]
        public async Task UpdateUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            // Create user with role and orgunit (orgunit contain role)
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

            var getUserDto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUserDto.Name = "Johnny";

            var updatedUser = await _omUserAppService.UpdateUserInTenantAsync(tenantDto.Id, getUserDto);
            updatedUser.FullName.ShouldBe("Johnny Smith");
        }

        [Fact]
        public async Task DeleteUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            // Create user with role and orgunit (orgunit contain role)
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

            var getUserDto = await _omUserAppService.GetUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            getUserDto.Name = "Johnny";

            await _omUserAppService.DeleteUserInTenantAsync(tenantDto.Id, new EntityDto<long>(userDto.Id));
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.TenantId == tenantDto.Id);
                user.IsDeleted.ShouldBeTrue();
            });
        }

        [Fact]
        public async Task ActivateUserInTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

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

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);
                user.FullName.ShouldBe("John Smith");
                user.IsActive.ShouldBeTrue();
                user.TenantId.ShouldBe(tenantDto.Id);
            });

            ActivateUserDto activateUserDto = new ActivateUserDto()
            {
                UserId = userDto.Id,
                IsActive = false
            };
            var result = await _omUserAppService.ActivateUserInTenantAsync(tenantDto.Id, activateUserDto);

            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);
                user.FullName.ShouldBe("John Smith");
                user.IsActive.ShouldBeFalse();
            });
        }
    }
}
