using Abp.Application.Services;
using Abp.Application.Services.Dto;
using CharonX.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.IdentityFramework;
using Abp.Organizations;
using Abp.UI;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Organizations;
using Microsoft.AspNetCore.Identity;

namespace CharonX.Users
{
    public class OmUserAppService : ApplicationService, IOmUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;

        public OmUserAppService(
            UserManager userManager,
            RoleManager roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public async Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = ObjectMapper.Map<User>(input);

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                await CheckDuplicatedPhoneNumber(user);

                CheckErrors(await _userManager.CreateAsync(user, input.Password));

                CheckErrors(await _userManager.SetOrgUnitsAndRoles(user, input.OrgUnitNames, input.RoleNames));

                return await GetUserInTenantAsync(tenantId, new EntityDto<long>(user.Id));
            }
        }

        private async Task CheckDuplicatedPhoneNumber(User user)
        {
            if (await _userManager.CheckDuplicateMobilePhoneAsync(user.PhoneNumber))
            {
                throw new UserFriendlyException(L("PhoneNumberDuplicated", user.PhoneNumber));
            }
        }

        public async Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            input.OrgUnitNames = input.OrgUnitNames.Append(OrganizationUnitHelper.DefaultAdminOrgUnitName).ToArray();

            return await CreateUserInTenantAsync(tenantId, input);
        }

        public async Task<List<UserDto>> GetAllAdminUserInTenantAsync(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var adminRole = await _roleManager.GetRoleByNameAsync(StaticRoleNames.Tenants.Admin);

                IList<User> adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);

                List<UserDto> userDtos = new List<UserDto>();
                foreach (User adminUser in adminUsers)
                {
                    UserDto userDto = ObjectMapper.Map<UserDto>(adminUser);

                    userDto.OrgUnitNames = await GetOrgUnitsOfUserAsync(adminUser);
                    userDto.RoleNames = await GetRolesOfUserAsync(adminUser);
                    userDto.Permissions = await GetPermissionsOfUserAsync(adminUser);

                    userDtos.Add(userDto);
                }

                return userDtos;
            }
        }

        public async Task<UserDto> GetUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(input.Id);
                    var userDto = ObjectMapper.Map<UserDto>(user);

                    userDto.OrgUnitNames = await GetOrgUnitsOfUserAsync(user);
                    userDto.RoleNames = await GetRolesOfUserAsync(user);
                    userDto.Permissions = await GetPermissionsOfUserAsync(user);

                    return userDto;
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }
            }
        }

        private async Task<string[]> GetOrgUnitsOfUserAsync(User user)
        {
            var orgUnits = await _userManager.GetOrganizationUnitsAsync(user);
            return orgUnits.Select(ou => ou.DisplayName).ToArray();
        }

        private async Task<string[]> GetRolesOfUserAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToArray();
        }

        private async Task<string[]> GetPermissionsOfUserAsync(User user)
        {
            var permissions = await _userManager.GetGrantedPermissionsAsync(user);
            return permissions.Select(p => p.Name).ToArray();
        }

        public async Task<UserDto> UpdateUserInTenantAsync(int tenantId, UserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                User user;

                try
                {
                    user = await _userManager.GetUserByIdAsync(input.Id);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }

                if (input.PhoneNumber != user.PhoneNumber)
                {
                    await CheckDuplicatedPhoneNumber(user);
                }

                MapToEntity(input, user);

                CheckErrors(await _userManager.UpdateAsync(user));

                CheckErrors(await _userManager.SetOrgUnitsAndRoles(user, input.OrgUnitNames, input.RoleNames));

                return await GetUserInTenantAsync(tenantId, new EntityDto<long>(user.Id));
            }
        }

        protected void MapToEntity(UserDto input, User user)
        {
            //ObjectMapper.Map(input, user);
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.Gender = input.Gender;
            user.IdNumber = input.IdNumber;
            user.PhoneNumber = input.PhoneNumber;
            user.OfficePhoneNumber = input.OfficePhoneNumber;
            user.City = input.City;
            user.ExpireDate = input.ExpireDate;
            user.EmailAddress = input.EmailAddress;
            user.IsActive = input.IsActive;

            user.SetNormalizedNames();
        }

        public async Task DeleteUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                User user;

                try
                {
                    user = await _userManager.GetUserByIdAsync(input.Id);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }

                CheckErrors(await _userManager.DeleteAsync(user));
            }
        }

        public async Task<bool> ActivateUserInTenantAsync(int tenantId, ActivateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = input.IsActive;

                CheckErrors(await _userManager.UpdateAsync(user));

                return true;
            }
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
