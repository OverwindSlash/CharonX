using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Organizations;
using CharonX.Organizations;
using CharonX.Organizations.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace CharonX.Tests.Organizations
{
    public class OrgUnitAppService_Tests : CharonXTestBase
    {
        private readonly IOrgUnitAppService _orgUnitAppService;

        public OrgUnitAppService_Tests()
        {
            _orgUnitAppService = Resolve<IOrgUnitAppService>();

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
                testOu.DisplayName.ShouldBe("Ou Test");
                testOu.Code.ShouldBe("00001");
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
            result.DisplayName.ShouldBe("Ou Test");
            result.Code.ShouldBe("00001");
        }
    }
}
