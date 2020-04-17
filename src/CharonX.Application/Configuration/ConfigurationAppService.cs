using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using CharonX.Configuration.Dto;

namespace CharonX.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CharonXAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
