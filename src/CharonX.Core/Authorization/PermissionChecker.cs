using Abp.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;

namespace CharonX.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
