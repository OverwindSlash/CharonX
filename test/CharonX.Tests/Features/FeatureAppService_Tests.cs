using CharonX.Features;
using CharonX.Features.Dto;
using CharonX.MultiTenancy;
using CharonX.MultiTenancy.Dto;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CharonX.Tests.Features
{
    public class FeatureAppService_Tests : CharonXTestBase
    {
        private readonly IFeatureAppService _featureAppService;
        private readonly ITenantAppService _tenantAppService;

        public FeatureAppService_Tests()
        {
            _featureAppService = Resolve<IFeatureAppService>();
            _tenantAppService = Resolve<ITenantAppService>();

            LoginAsHostAdmin();
        }

        [Fact]
        public void ListAllFeatures_Test()
        {
            var features = _featureAppService.ListAllFeatures();
            features.Count.ShouldBe(2);
        }

        [Fact]
        public async Task ListAllFeaturesInTenant_NoFeature_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            var features = await _featureAppService.ListAllFeaturesInTenantAsync(tenantDto.Id);
            features.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetTenantPermissions_NoFeature_Test()
        {
            CreateTenantDto dto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(dto);

            var permissions = await _featureAppService.GetTenantPermissionsAsync(tenantDto.Id);
            permissions.Items.Count.ShouldBe(2);
        }

        [Fact]
        public async Task EnableFeatureForTenantAsync_Enable_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            EnableFeatureDto enableFeatureDto = new EnableFeatureDto()
            {
                TenantId = tenantDto.Id,
                FeatureNames = new List<string>() { "SmartPassFeature" }
            };

            bool result = await _featureAppService.EnableFeatureForTenantAsync(enableFeatureDto);
            result.ShouldBeTrue();

            var features = await _featureAppService.ListAllFeaturesInTenantAsync(tenantDto.Id);
            features.Count.ShouldBe(1);

            var permissions = await _featureAppService.GetTenantPermissionsAsync(tenantDto.Id);
            permissions.Items.Count.ShouldBe(83);
        }

        [Fact]
        public async Task EnableFeatureForTenantAsync_Disable_Test()
        {
            CreateTenantDto createTenantDto = new CreateTenantDto()
            {
                TenancyName = "TestTenant",
                Name = "TestTenant",
                AdminPhoneNumber = "13851400000",
                IsActive = true
            };
            var tenantDto = await _tenantAppService.CreateAsync(createTenantDto);

            var permissions0 = await _featureAppService.GetTenantPermissionsAsync(tenantDto.Id);
            permissions0.Items.Count.ShouldBe(2);

            // Enable two features.
            EnableFeatureDto enableFeature1Dto = new EnableFeatureDto()
            {
                TenantId = tenantDto.Id,
                FeatureNames = new List<string>() { "SmartPassFeature", "SmartSecurityFeature" }
            };

            bool result1 = await _featureAppService.EnableFeatureForTenantAsync(enableFeature1Dto);
            result1.ShouldBeTrue();

            var features1 = await _featureAppService.ListAllFeaturesInTenantAsync(tenantDto.Id);
            features1.Count.ShouldBe(2);

            var permissions1 = await _featureAppService.GetTenantPermissionsAsync(tenantDto.Id);
            permissions1.Items.Count.ShouldBe(84);

            // Reset feature to remain one left
            EnableFeatureDto enableFeature2Dto = new EnableFeatureDto()
            {
                TenantId = tenantDto.Id,
                FeatureNames = new List<string>() { "SmartSecurityFeature" }
            };

            bool result2 = await _featureAppService.EnableFeatureForTenantAsync(enableFeature2Dto);
            result2.ShouldBeTrue();

            var features2 = await _featureAppService.ListAllFeaturesInTenantAsync(tenantDto.Id);
            features2.Count.ShouldBe(1);

            var permissions2 = await _featureAppService.GetTenantPermissionsAsync(tenantDto.Id);
            permissions2.Items.Count.ShouldBe(3);
        }

        [Fact]
        public void GetAllPermissions_Test()
        {
            var allPermissions = _featureAppService.GetAllPermissionsInSystem();
            allPermissions.Items.Count.ShouldBe(85);
        }
    }
}
