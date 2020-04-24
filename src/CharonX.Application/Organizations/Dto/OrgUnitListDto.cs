using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;

namespace CharonX.Organizations.Dto
{
    public class OrgUnitListDto : EntityDto, IHasCreationTime
    {
        public long? ParentId { get; set; }

        public string DisplayName { get; set; }

        public string Code { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
