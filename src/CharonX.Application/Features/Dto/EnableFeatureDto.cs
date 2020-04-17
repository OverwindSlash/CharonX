using System.Collections.Generic;

namespace CharonX.Features.Dto
{
    public class EnableFeatureDto
    {
        public int TenantId { get; set; }
        public List<string> FeatureNames { get; set; }
    }
}
