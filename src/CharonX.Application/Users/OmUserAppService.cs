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
        private readonly IPasswordHasher<User> _passwordHasher;

        public OmUserAppService(
            UserManager userManager,
            RoleManager roleManager,
            IPasswordHasher<User> passwordHasher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;

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

                    userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(adminUser);
                    userDto.RoleNames = await _userManager.GetRolesOfUserAsync(adminUser);
                    userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(adminUser);

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

                    userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(user);
                    userDto.RoleNames = await _userManager.GetRolesOfUserAsync(user);
                    userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);

                    return userDto;
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }
            }
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

        public async Task ResetUserPasswordInTenantAsync(int tenantId, ResetTenantUserPasswordDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                if (user == null)
                {
                    return;
                }

                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            }
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
