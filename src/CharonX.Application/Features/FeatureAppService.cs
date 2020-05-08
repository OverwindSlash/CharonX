using Abp;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Features.Dto;
using CharonX.MultiTenancy;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CharonX.Features
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class FeatureAppService : ApplicationService, IFeatureAppService
    {
        private readonly IFeatureManager _featureManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IIocManager _iocManager;
        private readonly TenantManager _tenantManager;
        private readonly RoleManager _roleManager;
        private readonly IAuthorizationConfiguration _authorizationConfiguration;
        private readonly IRepository<FeatureSetting, long> _featureRepository;

        public FeatureAppService(
            IFeatureManager featureManager,
            ILocalizationManager localizationManager,
            IIocManager iocManager,
            TenantManager tenantManager,
            RoleManager roleManager,
            IRepository<FeatureSetting, long> featureRepository,
            IAuthorizationConfiguration authorizationConfiguration)
        {
            _featureManager = featureManager;
            _localizationManager = localizationManager;
            _iocManager = iocManager;
            _tenantManager = tenantManager;
            _roleManager = roleManager;
            _authorizationConfiguration = authorizationConfiguration;
            _featureRepository = featureRepository;
        }

        /// <summary>
        /// 获取全部功能包
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<FeatureDto> ListAllFeatures()
        {
            var features = _featureManager.GetAll();

            List<FeatureDto> dtos = features.Select(
                feature => new FeatureDto()
                {
                    Name = feature.Name, 
                    DisplayName = _localizationManager.GetString((LocalizableString) feature.DisplayName)
                }).ToList();

            return dtos;
        }
        /// <summary>
        /// 开通租户的权限包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> EnableFeatureForTenantAsync(EnableFeatureDto input)
        {
            var tenant = await _tenantManager.GetByIdAsync(input.TenantId);
            if (tenant == null)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", input.TenantId));
            }

            await SetTenantFeatureAsync(input, tenant);

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                await _roleManager.GrantAllPermissionToAdminRoleInTenant(tenant);
            }

            return true;
        }

        private async Task SetTenantFeatureAsync(EnableFeatureDto input, Tenant tenant)
        {
            await _tenantManager.ResetAllFeaturesAsync(tenant.Id);
            await CurrentUnitOfWork.SaveChangesAsync();
            await _tenantManager.SetFeatureValuesAsync(tenant.Id,
                input.FeatureNames.Select(f => new NameValue(f, "true")).ToArray());
        }
        /// <summary>
        /// 获取特定租户的全部功能包
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<FeatureDto>> ListAllFeaturesInTenantAsync(int tenantId)
        {
            var features = await _tenantManager.GetFeatureValuesAsync(tenantId);

            var featureDtos = new List<FeatureDto>();
            foreach (var feature in features)
            {
                if (feature.Value != "true") continue;

                Feature entity = _featureManager.Get(feature.Name);
                featureDtos.Add(new FeatureDto()
                {
                    Name = entity.Name,
                    DisplayName = _localizationManager.GetString((LocalizableString)entity.DisplayName)
                });
            }

            return featureDtos;
        }

        /// <summary>
        /// 获取当前租户的全部功能包
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAllowAnonymous]
        public async Task<List<FeatureDto>> ListAllFeaturesInCurrentTenantAsync()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return new List<FeatureDto>();
            }

            var features = await _tenantManager.GetFeatureValuesAsync(AbpSession.TenantId.Value);

            var featureDtos = new List<FeatureDto>();
            foreach (var feature in features)
            {
                if (feature.Value != "true") continue;

                Feature entity = _featureManager.Get(feature.Name);
                featureDtos.Add(new FeatureDto()
                {
                    Name = entity.Name,
                    DisplayName = _localizationManager.GetString((LocalizableString)entity.DisplayName)
                });
            }

            return featureDtos;
        }

        /// <summary>
        /// 获取系统管理员的所有权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ListResultDto<PermissionDto> GetAllPermissionsInSystem()
        {
            var permissions = PermissionManager.GetAllPermissions();

            return new ListResultDto<PermissionDto>(
                ObjectMapper.Map<List<PermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList());
        }

        /// <summary>
        /// 获取特定租户的所有权限
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ListResultDto<PermissionDto>> GetTenantPermissionsAsync(int tenantId)
        {
            Tenant tenant = await _tenantManager.GetAvailableTenantById(tenantId);

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                // TODO: Investigate why Permission.GetAllPermission() result not filtered by tenant
                PermissionManager permissionManager = new PermissionManager(_iocManager, _authorizationConfiguration, UnitOfWorkManager);
                permissionManager.Initialize();
                var permissions = permissionManager.GetAllPermissions();

                return new ListResultDto<PermissionDto>(
                    ObjectMapper.Map<List<PermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList());
            }
        }
    }
}
