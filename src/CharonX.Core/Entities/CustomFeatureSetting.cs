using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CharonX.Entities
{
    public class CustomFeatureSetting:Entity
    {
        public string Name { get; set; }
        public string LocalizationEn { get; set; }
        public string LocalizationZh { get; set; }
    }
}
