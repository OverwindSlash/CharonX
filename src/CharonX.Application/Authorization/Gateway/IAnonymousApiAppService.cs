using Abp.Application.Services;
using CharonX.Authorization.Gateway.Dto;
using System.Threading.Tasks;

namespace CharonX.Authorization.Gateway
{
    public interface IAnonymousApiAppService : IApplicationService
    {
        public Task AddAnonymousRouteAsync(SetAnonymousDto setAnonymousDto);

        public Task RemoveAnonymousRouteAsync(SetAnonymousDto setAnonymousDto);

        public Task ResetAnonymousRouteAsync(ResetAnonymousDto resetAnonymousDto);
    }
}
