using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Uow;
using Abp.IdentityFramework;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Organizations;
using CharonX.Users.Dto;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharonX.Users
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
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
        /// <summary>
        /// 运维专用：对指定租户创建一个用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = ObjectMapper.Map<User>(input);

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                await CheckDuplicatedPhoneNumber(user.PhoneNumber);
                await CheckDuplicatedEmail(user.EmailAddress);

                CheckErrors(await _userManager.CreateAsync(user, input.Password));

                CheckErrors(await _userManager.SetOrgUnitsAndRoles(user, input.OrgUnitNames, input.RoleNames));

                return await GetUserInTenantAsync(tenantId, new EntityDto<long>(user.Id));
            }
        }

        private async Task CheckDuplicatedPhoneNumber(string phoneNumber)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (await _userManager.CheckDuplicateMobilePhoneAsync(phoneNumber))
                {
                    throw new UserFriendlyException(L("PhoneNumberDuplicated", phoneNumber));
                }
            }
        }

        private async Task CheckDuplicatedEmail(string email)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                if (await _userManager.CheckDuplicateEmailAsync(email))
                {
                    throw new UserFriendlyException(L("EmailAddressDuplicated", email));
                }
            }
        }

        /// <summary>
        /// 运维专用：创建指定租户的管理员用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            input.OrgUnitNames = input.OrgUnitNames.Append(OrganizationUnitHelper.DefaultAdminOrgUnitName).ToArray();

            return await CreateUserInTenantAsync(tenantId, input);
        }
        /// <summary>
        /// 运维专用：获取指定租户的全部管理员用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        
        public async Task<PagedResultDto<UserDto>> GetAllAdminUserInTenantAsync(int tenantId, PagedAdminUserResultRequestDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var adminRole = await _roleManager.GetRoleByNameAsync(StaticRoleNames.Tenants.Admin);

                IList<User> adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    adminUsers = adminUsers.Where(
                        u => u.UserName.Contains(input.Keyword) || u.Name.Contains(input.Keyword) || u.EmailAddress.Contains(input.Keyword)).ToList();
                }

                if (input.IsActive.HasValue)
                {
                    adminUsers = adminUsers.Where(u => u.IsActive == input.IsActive.Value).ToList();
                }

                var query = adminUsers.AsQueryable();
                query = PagingHelper.ApplySorting<User, long>(query, input);
                query = PagingHelper.ApplyPaging<User, long>(query, input);
                adminUsers = query.ToList();

                List<UserDto> userDtos = new List<UserDto>();
                foreach (User adminUser in adminUsers)
                {
                    UserDto userDto = ObjectMapper.Map<UserDto>(adminUser);

                    userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(adminUser);
                    userDto.RoleNames = await _userManager.GetRolesOfUserAsync(adminUser);
                    userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                    userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(adminUser);

                    userDtos.Add(userDto);
                }

                return new PagedResultDto<UserDto>(userDtos.Count, userDtos);
            }
        }
        /// <summary>
        /// 运维专用：获取指定租户的某一用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
                    userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                    userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);

                    return userDto;
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.Id), exception);
                }
            }
        }
        /// <summary>
        /// 运维专用：更新指定租户的某一用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
                    await CheckDuplicatedPhoneNumber(user.PhoneNumber);
                }

                if (input.EmailAddress != user.EmailAddress)
                {
                    await CheckDuplicatedEmail(user.EmailAddress);
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
        /// <summary>
        /// 运维专用：删除指定租户的某一用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 运维专用：激活指定租户下的某一用户
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 运维专用：重置指定租户下某一用户的密码
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
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
