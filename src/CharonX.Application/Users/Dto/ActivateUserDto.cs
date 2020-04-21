using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CharonX.Users.Dto
{
    public class ActivateUserDto
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
