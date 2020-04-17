using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.MultiTenancy;
using CharonX.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace CharonX.MultiTenancy.Dto
{
    [AutoMapTo(typeof(Tenant))]
    public class CreateTenantDto
    {
        private string _adminEmailAddress;

        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        [RegularExpression(AbpTenantBase.TenancyNameRegex)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(Tenant.MaxPhoneNumberLength)]
        public string AdminPhoneNumber { get; set; }

        //[Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string AdminEmailAddress
        {
            get => string.IsNullOrEmpty(_adminEmailAddress) ? (AdminPhoneNumber + User.DefaultEmailDomain) : _adminEmailAddress;
            set => _adminEmailAddress = value;
        }

        [StringLength(Tenant.MaxContactLength)]
        public string Contact { get; set; }

        [StringLength(Tenant.MaxAddressLength)]
        public string Address { get; set; }

        [StringLength(Tenant.MaxLogoLength)]
        public string Logo { get; set; }

        //[StringLength(AbpTenantBase.MaxConnectionStringLength)]
        //public string ConnectionString { get; set; }

        public bool IsActive {get; set;}
    }
}
