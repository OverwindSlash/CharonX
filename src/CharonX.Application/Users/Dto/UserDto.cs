using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using CharonX.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CharonX.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [MaxLength(User.MaxGenderLength)]
        public string Gender { get; set; }

        [StringLength(User.MaxIdNumberLength)]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string OfficePhoneNumber { get; set; }

        [MaxLength(User.MaxCityLength)]
        public string City { get; set; }

        public DateTime ExpireDate { get; set; }

        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public bool? IsActive { get; set; }

        public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string[] OrgUnitNames { get; set; }

        public string[] RoleNames { get; set; }

        public string[] Permissions { get; set; }

        public bool? IsAdmin { get; set; }
    }
}
