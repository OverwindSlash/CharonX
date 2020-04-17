using System.ComponentModel.DataAnnotations;

namespace CharonX.MultiTenancy.Dto
{
    public class ActivateTenantDto
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
