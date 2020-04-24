using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Organizations;
using System.Collections.Generic;

namespace CharonX.Organizations.Dto
{
    [AutoMap(typeof(OrganizationUnit))]
    public class OrgUnitDto : EntityDto<long>
    {
        public long? ParentId { get; set; }

        public string DisplayName { get; set; }

        public string Code { get; set; }

        public List<string> AssignedRoles { get; set; }

        public List<string> GrantedPermissions { get; set; }
    }
}
