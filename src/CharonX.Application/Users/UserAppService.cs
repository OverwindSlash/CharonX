using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Accounts;
using CharonX.Authorization.AuthCode;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Domain.Uow;
using CharonX.MultiTenancy;

namespace CharonX.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly IRepository<User, long> _repository;
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly SmsAuthManager _smsAuthManager;

        public UserAppService(
            IRepository<User, long> repository,
            TenantManager tenantManager,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            SmsAuthManager smsAuthManager)
            : base(repository)
        {
            _repository = repository;
            _tenantManager = tenantManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _orgUnitRepository = orgUnitRepository;
            _smsAuthManager = smsAuthManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }
        /// <summary>
        /// 对当前租户创建一个用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            await CheckDuplicatedPhoneNumber(user.PhoneNumber);
            await CheckDuplicatedEmail(user.EmailAddress);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            CheckErrors(await _userManager.SetOrgUnitsAndRoles(user, input.OrgUnitNames, input.RoleNames));

            return await GetAsync(new EntityDto<long>(user.Id));
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
        /// 获取当前租户的某一用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<UserDto> GetAsync(EntityDto<long> input)
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

        /// <summary>
        /// 获取当前租户下的全部用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<PagedResultDto<UserDto>> GetAllAsync(PagedUserResultRequestDto input)
        {
            var pagedResult = await base.GetAllAsync(input);
            foreach (var userDto in pagedResult.Items)
            {
                var user = await _userManager.GetUserByIdAsync(userDto.Id);
                userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(user);
                userDto.RoleNames = await _userManager.GetRolesOfUserAsync(user);
                userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);
            }

            return pagedResult;
        }
        /// <summary>
        /// 更新当前租户的某一用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

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

            return await GetAsync(new EntityDto<long>(user.Id));
        }
        /// <summary>
        /// 删除当前租户的某一用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }
        /// <summary>
        /// 获取当前租户中指定角色下的所有用户
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersInRoleAsync(string roleName)
        {
            List<UserDto> userDtos = new List<UserDto>();

            var users = await _userManager.GetUsersInRoleAsync(roleName);
            foreach (User user in users)
            {
                UserDto userDto = ObjectMapper.Map<UserDto>(user);
                userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(user);
                userDto.RoleNames = await _userManager.GetRolesOfUserAsync(user);
                userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);
                userDtos.Add(userDto);
            }

            return userDtos;
        }
        /// <summary>
        /// 获取当前租户指定组织中的所有用户
        /// </summary>
        /// <param name="orgUnitName"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersInOrgUnitAsync(string orgUnitName, bool includeChildren = false)
        {
            List<UserDto> userDtos = new List<UserDto>();

            var orgUnit = await _orgUnitRepository.FirstOrDefaultAsync(ou => ou.DisplayName == orgUnitName);
            if (orgUnit == null)
            {
                return userDtos;
            }

            var users = await _userManager.GetUsersInOrganizationUnitAsync(orgUnit, includeChildren);
            foreach (User user in users)
            {
                UserDto userDto = ObjectMapper.Map<UserDto>(user);
                userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(user);
                userDto.RoleNames = await _userManager.GetRolesOfUserAsync(user);
                userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);
                userDtos.Add(userDto);
            }

            return userDtos;
        }
        /// <summary>
        /// 获取指定租户中某一用户的所有权限
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAllowAnonymous]
        public async Task<ListResultDto<string>> GetPermissions(GetPermissionsDto input)
        {
            using (UnitOfWorkManager.Current.SetTenantId(input.TenantId))
            {
                try
                {
                    var user = await _userManager.GetUserByIdAsync(input.UserId);
                    var userDto = ObjectMapper.Map<UserDto>(user);

                    userDto.OrgUnitNames = await _userManager.GetOrgUnitsOfUserAsync(user);
                    userDto.RoleNames = await _userManager.GetRolesOfUserAsync(user);
                    userDto.IsAdmin = userDto.RoleNames.Contains("Admin");
                    userDto.Permissions = await _userManager.GetPermissionsOfUserAsync(user);

                    return new ListResultDto<string>(userDto.Permissions);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(L("UserNotFound", input.UserId), exception);
                }
            }
        }

        // public async Task<ListResultDto<RoleDto>> GetRoles()
        // {
        //     var roles = await _roleRepository.GetAllListAsync();
        //     return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        // }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
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

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        /// <summary>
        /// 修改当前登录用户的密码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAllowAnonymous]
        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to change password.");
            }

            long userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);

            string tenancyName = null;
            int? tenantId = AbpSession.TenantId;
            if (tenantId.HasValue)
            {
                Tenant tenant = await _tenantManager.GetByIdAsync(tenantId.Value);
                tenancyName = tenant.TenancyName;
            }

            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, tenancyName: tenancyName, shouldLockout: false);

            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException(L("ExistingPasswordWrong"));
            }

            //if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
            //{
            //    throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
            //}

            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }

        /// <summary>
        /// 激活指定用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<bool> ActivateUser(ActivateUserDto input)
        {
            var user = await Repository.GetAsync(input.UserId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = input.IsActive;

            CheckErrors(await _userManager.UpdateAsync(user));

            return true;
        }
        /// <summary>
        /// 通过当前租户管理员修改指定用户密码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> ResetUserPasswordByTenantAdmin(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException(L("NeedLoginAsTenantAdmin"));
            }

            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException(L("NeedLoginAsTenantAdmin"));
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();
            }

            return true;
        }
        /// <summary>
        /// 通过短信重置指定用户的密码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> ResetSelfPasswordBySms(SmsResetPasswordDto input)
        {
            if (!await _smsAuthManager.AuthenticateSmsCode(input.PhoneNumber, input.AuthCode))
            {
                throw new UserFriendlyException("Wrong authentication code.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user == null)
            {
                throw new UserFriendlyException("User not exist.");
            }

            if (user.PhoneNumber != input.PhoneNumber)
            {
                throw new UserFriendlyException("Wrong mobile phone number.");
            }

            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }
    }
}

