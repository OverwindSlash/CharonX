using System;

namespace CharonX
{
    public class CharonXConsts
    {
        public const string LocalizationSourceName = "CharonX";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;

        public static string DefaultAvatarBase64;

        static CharonXConsts()
        {
            string path = @"Avatar/user.png";
            byte[] b = System.IO.File.ReadAllBytes(path);
            DefaultAvatarBase64 = "data:image/png;base64," + Convert.ToBase64String(b);
        }
    }
}
