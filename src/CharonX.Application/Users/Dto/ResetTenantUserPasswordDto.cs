using System;
using System.Collections.Generic;
using System.Text;

namespace CharonX.Users.Dto
{
    public class ResetTenantUserPasswordDto
    {
        public long UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
