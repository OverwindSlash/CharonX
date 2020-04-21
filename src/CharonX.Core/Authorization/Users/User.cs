using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.Extensions;

namespace CharonX.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";
        public const string DefaultEmailDomain = "@pensees.ai";
        public const int MaxGenderLength = 2;
        public const int MaxIdNumberLength = 18;
        public const int MaxCityLength = 10;

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string OfficePhoneNumber { get; set; }

        [MaxLength(MaxGenderLength)]
        public string Gender { get; set; }

        [StringLength(MaxIdNumberLength)]
        public string IdNumber { get; set; }

        [MaxLength(MaxCityLength)]
        public string City { get; set; }

        public DateTime ExpireDate { get; set; }
    }
}
