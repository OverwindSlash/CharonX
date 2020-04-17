using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace CharonX.Controllers
{
    public abstract class CharonXControllerBase: AbpController
    {
        protected CharonXControllerBase()
        {
            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
