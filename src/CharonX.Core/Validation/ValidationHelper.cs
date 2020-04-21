using System.Text.RegularExpressions;
using Abp.Extensions;

namespace CharonX.Validation
{
    public static class ValidationHelper
    {
        public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string MobileRegex = @"^1[3456789]\d{9}$";

        public static bool IsEmail(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return false;
            }

            var regex = new Regex(EmailRegex);
            return regex.IsMatch(value);
        }

        public static bool IsMobilePhone(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return false;
            }

            var regex = new Regex(MobileRegex);
            return regex.IsMatch(value);
        }
    }
}
