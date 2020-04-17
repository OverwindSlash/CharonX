using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Features.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharonX.Features
{
    public interface IFeatureAppService : IApplicationService
    {
        public List<FeatureDto> ListAllFeatures();

        public Task<bool> EnableFeatureForTenantAsync(EnableFeatureDto input);

        public Task<List<FeatureDto>> ListAllFeaturesInTenantAsync(int tenantId);

        public ListResultDto<PermissionDto> GetAllPermissions();

        public Task<ListResultDto<PermissionDto>> GetTenantPermissionsAsync(int tenantId);

    }
}
