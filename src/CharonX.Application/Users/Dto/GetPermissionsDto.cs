using System.ComponentModel.DataAnnotations;

namespace CharonX.Users.Dto
{
    public class GetPermissionsDto
    {
        public int? TenantId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
