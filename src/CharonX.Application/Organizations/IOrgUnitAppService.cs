using Abp.Application.Services;
using CharonX.Organizations.Dto;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace CharonX.Organizations
{
    public interface IOrgUnitAppService : IApplicationService
    {
        public Task<OrgUnitDto> CreateAsync(CreateOrgUnitDto input);
        public Task<OrgUnitDto> GetAsync(EntityDto<long> input);
    }
}
