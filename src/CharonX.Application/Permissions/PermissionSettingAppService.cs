using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Domain.Repositories;
using CharonX.Entities;
using CharonX.Permissions.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CharonX.Permissions
{
    /// <summary>
    /// 自定义权限
    /// </summary>
    public class PermissionSettingAppService : ApplicationService, IPermissionSettingAppService
    {
        private readonly IRepository<CustomPermissionSetting> permissionRepository;
        private readonly IRepository<CustomFeatureSetting> featureRepository;

        public PermissionSettingAppService(IRepository<CustomPermissionSetting> permissionRepository,
            IRepository<CustomFeatureSetting> featureRepository)
        {
            this.permissionRepository = permissionRepository;
            this.featureRepository = featureRepository;
        }
        /// <summary>
        /// 新建自定义权限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomPermissionSettingDto> CreatePermission(CustomPermissionSettingDto input)
        {
            var setting = await permissionRepository.InsertAsync(ObjectMapper.Map<CustomPermissionSetting>(input));
            return ObjectMapper.Map<CustomPermissionSettingDto>(setting);
        }
        /// <summary>
        /// 获取单个权限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomPermissionSettingDto> GetPermission(EntityDto input)
        {
            var setting = await permissionRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            if (setting==null)
            {
                return new CustomPermissionSettingDto();
            }
            return ObjectMapper.Map<CustomPermissionSettingDto>(setting);
        }
        /// <summary>
        /// 获取全部权限
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomPermissionSettingDto>> GetAllPermissions()
        {
            var settings = await permissionRepository.GetAllListAsync();
            return ObjectMapper.Map<List<CustomPermissionSettingDto>>(settings);
        }
        /// <summary>
        /// 更新指定权限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomPermissionSettingDto> UpdatePermission(CustomPermissionSettingDto input)
        {
            var setting= await permissionRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            ObjectMapper.Map(input,setting);
            return input;
        }
        /// <summary>
        /// 自定义功能包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomFeatureSettingDto> CreateFeature(CustomFeatureSettingDto input)
        {
            var setting = await featureRepository.InsertAsync(ObjectMapper.Map<CustomFeatureSetting>(input));
            return ObjectMapper.Map<CustomFeatureSettingDto>(setting);
        }
        /// <summary>
        /// 更新指定功能包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomFeatureSettingDto> UpdateFeature(CustomFeatureSettingDto input)
        {
            var setting = await featureRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            ObjectMapper.Map(input, setting);
            return input;
        }
        /// <summary>
        /// 获取指定功能包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CustomFeatureSettingDto> GetFeature(EntityDto input)
        {
            var setting = await featureRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            if (setting == null)
            {
                return new CustomFeatureSettingDto();
            }
            return ObjectMapper.Map<CustomFeatureSettingDto>(setting);
        }
        /// <summary>
        /// 获取全部功能包
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomFeatureSettingDto>> GetAllFeatures()
        {
            var settings = await featureRepository.GetAllListAsync();
            return ObjectMapper.Map<List<CustomFeatureSettingDto>>(settings);
        }

        //private Task CreateOrUpdateXmlNode()
        //{
        //    XDocument xmlFile=XDocument.Load()
        //}
    }
}
