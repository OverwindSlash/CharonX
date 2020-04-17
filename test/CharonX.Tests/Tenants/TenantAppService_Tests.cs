using Abp.Application.Services.Dto;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace CharonX.Tests.Tenants
{
    public class TenantAppService_Tests : CharonXTestBase
    {
        private ITenantAppService _tenantAppService;

        public TenantAppService_Tests()
        {
            _tenantAppService = Resolve<ITenantAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public async Task CreateTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var result = await _tenantAppService.CreateAsync(dto);

            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Tenants.FirstOrDefaultAsync(u => u.TenancyName == dto.TenancyName);
                testTenant.ShouldNotBeNull();
            });
        }

        [Fact]
        public async Task GetTenantById_Test()
        {
            var result = await _tenantAppService.GetAsync(new EntityDto<int>(1));
            result.TenancyName.ShouldBe("Default");
        }


        [Fact]
        public async Task GetAllTenants_Test()
        {
            var result = await _tenantAppService.GetAllAsync(new PagedTenantResultRequestDto
                { MaxResultCount = 20, SkipCount = 0 });

            result.TotalCount.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var createResult = await _tenantAppService.CreateAsync(dto);
            createResult.Name = "NewTenant";
            createResult.Contact = "ContactName";
            createResult.Address = "TestAddress";
            createResult.Logo = "LogoUrl";

            var updateResult = await _tenantAppService.UpdateAsync(createResult);

            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Tenants.FirstOrDefaultAsync(u => u.TenancyName == dto.TenancyName);
                testTenant.Name.ShouldBe("NewTenant");
                testTenant.Contact.ShouldBe("ContactName");
                testTenant.Address.ShouldBe("TestAddress");
                testTenant.Logo.ShouldBe("LogoUrl");
            });

        }

        [Fact]
        public async Task DeleteTenant_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var createResult = await _tenantAppService.CreateAsync(dto);

            await _tenantAppService.DeleteAsync(new EntityDto<int>(createResult.Id));
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Tenants.FirstOrDefaultAsync(u => u.TenancyName == dto.TenancyName);
                testTenant.IsDeleted = true;
            });
        }

        [Fact]
        public async Task ActivateTenant_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var createResult = await _tenantAppService.CreateAsync(createTenantDto);

            // Deactivate
            ActivateTenantDto activateTenantDto1 = new ActivateTenantDto()
            {
                TenantId = createResult.Id,
                IsActive = false
            };

            var activateResult1 = await _tenantAppService.ActivateTenant(activateTenantDto1);
            activateResult1.ShouldBeTrue();
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Tenants.FirstOrDefaultAsync(u => u.TenancyName == createTenantDto.TenancyName);
                testTenant.IsActive = false;
            });

            // Activate
            ActivateTenantDto activateTenantDto2 = new ActivateTenantDto()
            {
                TenantId = createResult.Id,
                IsActive = true
            };
            var activateResult2 = await _tenantAppService.ActivateTenant(activateTenantDto2);
            activateResult2.ShouldBeTrue();
            await UsingDbContextAsync(async context =>
            {
                var testTenant = await context.Tenants.FirstOrDefaultAsync(u => u.TenancyName == createTenantDto.TenancyName);
                testTenant.IsActive = true;
            });
        }
    }
}
