using System.Threading.Tasks;
using Abp.Application.Services;
using CharonX.Sessions.Dto;

namespace CharonX.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

        Task<GetCurrentLoginInformationsForAppOutput> GetCurrentLoginInformationsForApp();
    }
}
