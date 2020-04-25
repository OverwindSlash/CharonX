using System.ComponentModel.DataAnnotations;

namespace CharonX.Models
{
    public class GetTenantAdminToken
    {
        [Required]
        public int TenantId { get; set; }
    }
}
