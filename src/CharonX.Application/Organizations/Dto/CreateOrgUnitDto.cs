using Abp.AutoMapper;
using Abp.Organizations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CharonX.Organizations.Dto
{
    [AutoMap(typeof(OrganizationUnit))]
    public class CreateOrgUnitDto
    {
        public long? ParentId { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string Code { get; set; }
    }
}
