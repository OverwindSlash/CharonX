using System;
using System.Collections.Generic;
using System.Text;

namespace CharonX.Sessions.Dto
{
    public class GetCurrentLoginInformationsForAppOutput
    {
        public long UserId { get; set; }

        public string Username { get; set; }

        public string AvatarBase64 { get; set; }

        public IList<string> Roles { get; set; }

        public string TenancyName { get; set; }
    }
}
