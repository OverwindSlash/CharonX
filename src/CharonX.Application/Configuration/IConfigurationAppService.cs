using System.Threading.Tasks;
using CharonX.Configuration.Dto;

namespace CharonX.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
