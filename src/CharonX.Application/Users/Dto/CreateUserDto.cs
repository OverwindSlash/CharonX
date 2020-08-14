using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using CharonX.Authorization.Users;
using CharonX.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace CharonX.Users.Dto
{
    [AutoMapTo(typeof(User))]
    public class CreateUserDto : IShouldNormalize, ICustomValidate
    {
        private const string EmailDomain = @"@pensees.ai";
        private string _emailAddress;

        public CreateUserDto()
        {
            ExpireDate = DateTime.MaxValue;
        }

        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [MaxLength(2)]
        public string Gender { get; set; }

        [StringLength(18)]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string OfficePhoneNumber { get; set; }

        [MaxLength(10)]
        public string City { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }
        //public string EmailAddress
        //{
        //    get => string.IsNullOrEmpty(_emailAddress) ? (PhoneNumber + EmailDomain) : _emailAddress;
        //    set => _emailAddress = value;
        //}

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }

        public string[] OrgUnitNames { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public DateTime ExpireDate { get; set; }

        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }

            if (OrgUnitNames == null)
            {
                OrgUnitNames = new string[0];
            }
        }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (!ValidationHelper.IsMobilePhone(PhoneNumber))
            {
                context.Results.Add(new ValidationResult("Invalid mobile phone number!"));
            }
        }
    }
}
