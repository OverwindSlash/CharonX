using Abp.Application.Services;
using CharonX.MultiTenancy.Dto;
using System.Threading.Tasks;

namespace CharonX.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
        Task<bool> ActivateTenant(ActivateTenantDto input);
    }
}

