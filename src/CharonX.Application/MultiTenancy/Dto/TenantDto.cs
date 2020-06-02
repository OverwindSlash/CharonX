using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.MultiTenancy;
using System;
using System.ComponentModel.DataAnnotations;

namespace CharonX.MultiTenancy.Dto
{
    [AutoMap(typeof(Tenant))]
    public class TenantDto : EntityDto
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        [RegularExpression(AbpTenantBase.TenancyNameRegex)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(Tenant.MaxContactLength)]
        public string Contact { get; set; }
  
        [StringLength(Tenant.MaxPhoneNumberLength)]
        public string AdminPhoneNumber { get; set; }

        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string AdminEmailAddress { get; set; }

        [StringLength(Tenant.MaxAddressLength)]
        public string Address { get; set; }

        [StringLength(Tenant.MaxLogoLength)]
        public string Logo { get; set; }

        [StringLength(Tenant.MaxLogoNodeLength)]
        public string LogoNode { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
