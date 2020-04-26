using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.MultiTenancy;

namespace CharonX.MultiTenancy.Dto
{
    [AutoMapFrom(typeof(Tenant))]
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

        [StringLength(Tenant.MaxAddressLength)]
        public string Address { get; set; }

        [StringLength(Tenant.MaxLogoLength)]
        public string Logo { get; set; }

        [StringLength(Tenant.MaxLogoNodeLength)]
        public string LogoNode { get; set; }

        public bool IsActive { get; set; }
    }
}
