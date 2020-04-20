using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Organizations;
using CharonX.Organizations.Dto;
using System.Threading.Tasks;

namespace CharonX.Organizations
{
    public class OrgUnitAppService : ApplicationService, IOrgUnitAppService
    {
        private readonly OrganizationUnitManager _orgUnitManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;

        public OrgUnitAppService(
            OrganizationUnitManager orgUnitManager,
            IRepository<OrganizationUnit, long> orgUnitRepository
            )
        {
            _orgUnitManager = orgUnitManager;
            _orgUnitRepository = orgUnitRepository;
        }

        public async Task<OrgUnitDto> CreateAsync(CreateOrgUnitDto input)
        {
            var orgUnit = ObjectMapper.Map<OrganizationUnit>(input);
            orgUnit.TenantId = GetCurrentTenantId();

            await _orgUnitManager.CreateAsync(orgUnit);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<OrgUnitDto>(orgUnit);
        }

        public async Task<OrgUnitDto> GetAsync(EntityDto<long> input)
        {
            var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.Id == input.Id);

            return ObjectMapper.Map<OrgUnitDto>(orgUnit);
        }

        private int? GetCurrentTenantId()
        {
            if (CurrentUnitOfWork != null)
            {
                return CurrentUnitOfWork.GetTenantId();
            }

            return AbpSession.TenantId;
        }
    }
}
