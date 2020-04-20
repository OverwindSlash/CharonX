using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Organizations;

namespace CharonX.Organizations.Dto
{
    [AutoMap(typeof(OrganizationUnit))]
    public class OrgUnitDto : EntityDto<long>
    {
        public long? ParentId { get; set; }

        public string DisplayName { get; set; }

        public string Code { get; set; }
    }
}
