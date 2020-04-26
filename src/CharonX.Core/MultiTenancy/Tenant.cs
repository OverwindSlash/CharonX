using Abp.MultiTenancy;
using CharonX.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace CharonX.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public const int MaxLogoLength = 256;
        public const int MaxLogoNodeLength = 256;
        public const int MaxContactLength = 128;
        public const int MaxPhoneNumberLength = 32;
        public const int MaxAddressLength = 256;

        [Required]
        [StringLength(MaxPhoneNumberLength)]
        public string AdminPhoneNumber { get; set; }

        [StringLength(MaxContactLength)]
        public string Contact { get; set; }

        [StringLength(MaxAddressLength)]
        public string Address { get; set; }

        [StringLength(MaxLogoLength)]
        public string Logo { get; set; }

        [StringLength(MaxLogoNodeLength)]
        public string LogoNode { get; set; }

        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
