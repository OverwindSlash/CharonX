﻿using System;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using CharonX.Authorization;
using CharonX.Authorization.Accounts;
using CharonX.Authorization.Roles;
using CharonX.Authorization.Users;
using CharonX.Roles.Dto;
using CharonX.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CharonX.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly IRepository<User, long> _repository;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;

        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager)
            : base(repository)
        {
            _repository = repository;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;

            LocalizationSourceName = CharonXConsts.LocalizationSourceName;
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            await CheckDuplicatedPhoneNumber(user);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            // Set organization units and roles belongs to them
            if (input.OrgUnitNames != null)
            {
                CheckErrors(await _userManager.SetOrgUnitsAsync(user, input.OrgUnitNames));
                CurrentUnitOfWork.SaveChanges();
            }

            // Add additional roles not included in organization units
            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.AddToAdditionalRolesAsync(user, input.RoleNames));
                CurrentUnitOfWork.SaveChanges();
            }

            return await GetAsync(new EntityDto<long>(user.Id));
        }

        

        private async Task CheckDuplicatedPhoneNumber(User user)
        {
            if (await _userManager.CheckDuplicateMobilePhoneAsync(user.PhoneNumber))
            {
                throw new UserFriendlyException(L("PhoneNumberDuplicated", user.PhoneNumber));
            }
        }

        public override async Task<UserDto> GetAsync(EntityDto<long> input)
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

        public override async Task<PagedResultDto<UserDto>> GetAllAsync(PagedUserResultRequestDto input)
        {
            var pagedResult = await base.GetAllAsync(input);
            foreach (var userDto in pagedResult.Items)
            {
                var user = await _userManager.GetUserByIdAsync(userDto.Id);
                userDto.OrgUnitNames = await GetOrgUnitsOfUserAsync(user);
                userDto.RoleNames = await GetRolesOfUserAsync(user);
                userDto.Permissions = await GetPermissionsOfUserAsync(user);
            }

            return pagedResult;
        }

        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);

            if (input.PhoneNumber != user.PhoneNumber)
            {
                await CheckDuplicatedPhoneNumber(user);
            }

            MapToEntity(input, user);   

            CheckErrors(await _userManager.UpdateAsync(user));

            // Set organization units and roles belongs to them
            if (input.OrgUnitNames != null)
            {
                CheckErrors(await _userManager.SetOrgUnitsAsync(user, input.OrgUnitNames));
                CurrentUnitOfWork.SaveChanges();
            }

            // Add additional roles not included in organization units
            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.AddToAdditionalRolesAsync(user, input.RoleNames));
                CurrentUnitOfWork.SaveChanges();
            }

            return await GetAsync(new EntityDto<long>(user.Id));
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

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

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to change password.");
            }
            long userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);
            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Existing Password' did not match the one on record.  Please try again or contact an administrator for assistance in resetting your password.");
            }
            if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
            {
                throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
            }
            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }

        public Task<bool> ActivateUser(ActivateUserDto input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to reset password.");
            }
            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();
            }

            return true;
        }
    }
}

