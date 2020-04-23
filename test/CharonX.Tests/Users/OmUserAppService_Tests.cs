using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using CharonX.Authorization.Users;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using CharonX.Users;
using CharonX.Users.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CharonX.Tests.Users
{
    public class OmUserAppService_Tests : CharonXTestBase
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IOmUserAppService _omUserAppService;
        private readonly IUserAppService _userAppService;

        public OmUserAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();
            _omUserAppService = Resolve<IOmUserAppService>();
            _userAppService = Resolve<IUserAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task CreateUserInTenant_Test()
        {
            LoginAsHostAdmin();

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
                PhoneNumber = "13851400000",
                OfficePhoneNumber = "025-86328888",
                City = "Nanjing",
                EmailAddress = "test@test.com",
                ExpireDate = new DateTime(2050, 12, 31),
                IsActive = true
            };

            var userDto = await _omUserAppService.CreateUserInTenantAsync(tenantDto.Id, createUserDto);

            await UsingDbContextAsync(tenantDto.Id, async context =>
            {
                //var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.TenantId == tenantDto.Id);
                var user =  await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
                user.FullName.ShouldBe("John Smith");
            });

            await UsingDbContextAsync(1, async context =>
            {
                //var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.TenantId == 1);
                var user = await _userAppService.GetAsync(new EntityDto<long>(userDto.Id));
                user.FullName.ShouldBe("John Smith");
            });
        }
    }
}
