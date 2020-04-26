using Abp.Auditing;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Sessions.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharonX.MultiTenancy;

namespace CharonX.Sessions
{
    public class SessionAppService : CharonXAppServiceBase, ISessionAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public SessionAppService(
            UserManager userManager,
            RoleManager roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            var output = new GetCurrentLoginInformationsOutput
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>()
                }
            };

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {
                output.User = ObjectMapper.Map<UserLoginInfoDto>(await GetCurrentUserAsync());
            }

            return output;
        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsForAppOutput> GetCurrentLoginInformationsForApp()
        {
            var output = new GetCurrentLoginInformationsForAppOutput();

            if (AbpSession.UserId.HasValue)
            {
                User user = await GetCurrentUserAsync();
                output.UserId = user.Id;
                output.Fullname = user.FullName;
                output.Surname = user.Surname;
                output.Name = user.Name;
                output.AvatarBase64 = CharonXConsts.DefaultAvatarBase64;

                List<string> roleDisplayNames = new List<string>();
                IList<string> roleNames = await _userManager.GetRolesAsync(user);
                foreach (string roleName in roleNames)
                {
                    Role role = await _roleManager.GetRoleByNameAsync(roleName);
                    roleDisplayNames.Add(role.DisplayName);
                }

                output.Roles = roleDisplayNames;
            }

            if (AbpSession.TenantId.HasValue)
            {
                Tenant tenant = await GetCurrentTenantAsync();
                output.TenantName = tenant.TenancyName;
            }

            return output;
        }
    }
}
