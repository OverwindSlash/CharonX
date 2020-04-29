using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Permissions.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CharonX.Permissions
{
    public interface IPermissionSettingAppService:IApplicationService
    {

        Task<CustomPermissionSettingDto> CreatePermission(CustomPermissionSettingDto input);
        Task<CustomPermissionSettingDto> UpdatePermission(CustomPermissionSettingDto input);
        Task<CustomPermissionSettingDto> GetPermission(EntityDto input);
        Task<List<CustomPermissionSettingDto>> GetAllPermissions();

        Task<CustomFeatureSettingDto> CreateFeature(CustomFeatureSettingDto input);
        Task<CustomFeatureSettingDto> UpdateFeature(CustomFeatureSettingDto input);
        Task<CustomFeatureSettingDto> GetFeature(EntityDto input);
        Task<List<CustomFeatureSettingDto>> GetAllFeatures();

    }
}
