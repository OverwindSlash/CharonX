using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using CharonX.Authentication.External;
using CharonX.Authentication.JwtBearer;
using CharonX.Authorization;
using CharonX.Authorization.AuthCode;
using CharonX.Authorization.Users;
using CharonX.Models;
using CharonX.Models.TokenAuth;
using CharonX.MultiTenancy;
using CharonX.ResulFilter;
using CharonX.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CharonX.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : CharonXControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IRepository<User, long> _repository;
        private readonly SmsAuthManager _smsAuthManager;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IRepository<User, long> repository,
            SmsAuthManager smsAuthManager)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _repository = repository;
            _smsAuthManager = smsAuthManager;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var tenantId = GetTenantId(loginResult);

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        private static int? GetTenantId(AbpLoginResult<Tenant, User> loginResult)
        {
            int? tenantId = null;
            if (loginResult.Tenant != null)
            {
                tenantId = loginResult.Tenant.Id;
            }

            return tenantId;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> AuthenticateWithSms([FromBody] SmsAuthenticateModel model)
        {
            string username = string.Empty;
            string tenantName = string.Empty;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _repository.GetAll().FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                if (user == null)
                {
                    throw new UserFriendlyException(L("PhoneNumberNotExist", model.PhoneNumber));
                }
                username = user.UserName;

                if (user.TenantId.HasValue)
                {
                    tenantName = this._tenantCache.Get(user.TenantId.Value).TenancyName;
                }
            }

            var loginResult = await GetLoginResultAsync(username, model.Password, tenantName);
            if (!await AuthenticateSmsCode(model.PhoneNumber, model.SmsAuthCode))
            {
                throw new UserFriendlyException(L("WrongSmsAuthCode"));
            }

            int? tenantId = null;
            if (loginResult.Tenant != null)
            {
                tenantId = loginResult.Tenant.Id;
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }
        [HttpPost]
        public async Task<AuthenticateResultModel> AuthenticateByEmail([FromBody] EmailAuthenticateModel model)
        {
            string username = string.Empty;
            string tenantName = string.Empty;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _repository.GetAll().FirstOrDefaultAsync(u => u.EmailAddress == model.EmailAdress);
                if (user == null)
                {
                    throw new UserFriendlyException(L("EmailAdressNotExist", model.EmailAdress));
                }
                username = user.UserName;

                if (user.TenantId.HasValue)
                {
                    tenantName = this._tenantCache.Get(user.TenantId.Value).TenancyName;
                }
            }

            var loginResult = await GetLoginResultAsync(username, model.Password, tenantName);

            int? tenantId = null;
            if (loginResult.Tenant != null)
            {
                tenantId = loginResult.Tenant.Id;
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }
        [HttpPost]
        public async Task<AuthenticateResultModel> AuthenticateForApp([FromBody] PhoneAuthenticateModel model)
        {
            string username = string.Empty;
            string tenantName = string.Empty;
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await _repository.GetAll().FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                if (user == null)
                {
                    throw new AppUserFriendlyException(L("PhoneNumberNotExist", model.PhoneNumber));
                }
                username = user.UserName;

                if (user.TenantId.HasValue)
                {
                    tenantName = this._tenantCache.Get(user.TenantId.Value).TenancyName;
                }
            }

            try
            {
                var loginResult = await GetLoginResultAsync(username, model.Password, tenantName);

                int? tenantId = null;
                if (loginResult.Tenant != null)
                {
                    tenantId = loginResult.Tenant.Id;
                }

                var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));

                return new AuthenticateResultModel
                {
                    AccessToken = accessToken,
                    EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                    ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                    UserId = loginResult.User.Id
                };
            }
            catch (UserFriendlyException e)
            {
                throw new AppUserFriendlyException(message:e.Message, details:e.Details);
            }
        }

        [HttpGet]
        public async Task GetSmsAuthenticationCode(string phoneNumber)
        {
            if (!ValidationHelper.IsMobilePhone(phoneNumber))
            {
                throw new UserFriendlyException(L("InvalidPhoneNumber", phoneNumber));
            }

            string authCode = await _smsAuthManager.GetSmsAuthCodeAsync(phoneNumber);
        }

        [HttpGet]
        public async Task<bool> AuthenticateSmsCode(string phoneNumber, string smsAuthCode)
        {
            if (!ValidationHelper.IsMobilePhone(phoneNumber))
            {
                throw new UserFriendlyException(L("InvalidPhoneNumber", phoneNumber));
            }

            return await _smsAuthManager.AuthenticateSmsCode(phoneNumber, smsAuthCode);
        }

        [HttpPost]
        [AbpAuthorize(PermissionNames.Pages_Tenants)]
        public async Task<string> GetTenantAdminToken([FromBody]GetTenantAdminToken input)
        {
            var tenancyName = _tenantCache.Get(input.TenantId).TenancyName;

            var loginResult = await GetLoginResultAsync(
                AbpUserBase.AdminUserName,
                CharonX.Authorization.Users.User.DefaultPassword,
                tenancyName
            );

            int? tenantId = null;
            if (loginResult.Tenant != null)
            {
                tenantId = loginResult.Tenant.Id;
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));

            return accessToken;
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            var tenantId = GetTenantId(loginResult);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity, tenantId)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);
            
            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    //check expireDate
                    if (loginResult.User.ExpireDate > DateTime.MinValue &&loginResult.User.ExpireDate<DateTime.Now)
                    {
                        throw new UserFriendlyException(L("UserExpired",loginResult.User.ExpireDate.ToShortDateString()));
                    }
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity, int? tenantId)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            string tenantIdStr = "null";
            if (tenantId.HasValue)
            {
                tenantIdStr = tenantId.ToString();
            }

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("tenantId", tenantIdStr)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}
