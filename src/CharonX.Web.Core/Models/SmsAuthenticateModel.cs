using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Abp.Authorization.Users;

namespace CharonX.Models
{
    public class SmsAuthenticateModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }

        [Required]
        [StringLength(6)]
        public string SmsAuthCode { get; set; }

        public bool RememberClient { get; set; }
    }
}
