using System.Threading.Tasks;
using Abp.Application.Services;
using CharonX.Authorization.Accounts.Dto;

namespace CharonX.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
