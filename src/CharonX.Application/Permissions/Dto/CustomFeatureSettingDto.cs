using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using CharonX.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CharonX.Permissions.Dto
{
    [AutoMap(typeof(CustomFeatureSetting))]
    public class CustomFeatureSettingDto:EntityDto
    {
        public string Name { get; set; }
        public string LocalizationEn { get; set; }
        public string LocalizationZh { get; set; }
    }
}
