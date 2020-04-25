using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Reflection.Extensions;
using Abp.Runtime.Caching;
using CharonX.Configuration;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace CharonX.Authorization.AuthCode
{
    public class SmsAuthManager : ISingletonDependency
    {
        public static string SmsAuthCodeCacheName = "SmsAuthCode";
        private const string SmsAuthCodeRetryTimesKey = ":Retried";
        private const string SmsSendApiName = "/sms/sendPinCode";

        private readonly ICacheManager _cacheManager;
        private readonly IConfigurationRoot _configuration;
        private readonly int _maxRetryTimes;
        private readonly string _smsServerUri;
        private readonly Random _random;

        public SmsAuthManager(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _random = new Random();
            _configuration = AppConfigurations.Get(typeof(CharonXCoreModule).GetAssembly().GetDirectoryPathOrNull());

            var value = _configuration["SmsAuthCode:MaxRetryTimes"];
            int _maxRetryTimes = 3;
            if (Int32.TryParse(value, out var result))
            {
                _maxRetryTimes = result;
            }

            _smsServerUri = _configuration["SmsAuthCode:SmsServerAddress"];
        }

        public async Task<string> GetSmsAuthCodeAsync(string phoneNumber)
        {
            ICache smsAuthCache = _cacheManager.GetCache(SmsAuthCodeCacheName);

            // Expire in 5 minutes.
            var authCode = await smsAuthCache.GetAsync(phoneNumber, () => GenerateSmsAuthCode());

            // Reset retry times.
            string retryKey = phoneNumber + SmsAuthCodeRetryTimesKey;
            await smsAuthCache.SetAsync(retryKey, 0);

            SendAuthCodeSms(phoneNumber, authCode);

            return authCode;
        }

        private void SendAuthCodeSms(string phoneNumber, string authCode)
        {
            string smsUri = _smsServerUri.Trim('/') + SmsSendApiName;

            var client = new RestClient(smsUri);
            IRestRequest request = new RestRequest();

            var param = new
            {
                phoneNumber = phoneNumber,
                pinCode = authCode
            };

            request.AddJsonBody(param);

            var response = client.Post(request);
        }

        public async Task<bool> AuthenticateSmsCode(string phoneNumber, string smsAuthCode)
        {
            ICache smsAuthCache = _cacheManager.GetCache(SmsAuthCodeCacheName);
            string retryKey = phoneNumber + SmsAuthCodeRetryTimesKey;

            string authCode = await smsAuthCache.GetAsync(phoneNumber, () => Task.FromResult(string.Empty));

            // Not get sms auth code yet, or retry times reach limitation.
            if (string.IsNullOrEmpty(authCode))
            {
                // TODO: If there is a better way to determine existence of a key
                await smsAuthCache.RemoveAsync(phoneNumber);
                return false;
            }

            // Auth code not match.
            if (smsAuthCode != authCode)
            {
                // Get retry times.
                int? retryTimes = await smsAuthCache.GetAsync(retryKey, () => null) as int?;

                // Already reach retry time limitation.
                if (!retryTimes.HasValue)
                {
                    return false;
                }

                // If >= 3, then reach retry time limitation, clean retry time cache and auth code cache.
                if (retryTimes.Value >= 3)
                {
                    await smsAuthCache.RemoveAsync(phoneNumber);
                    await smsAuthCache.RemoveAsync(retryKey);

                    return false;
                }

                // if < 3, then increase retry times and return false.
                await smsAuthCache.SetAsync(retryKey, ++retryTimes);
                return false;
            }

            // Auth code match.
            await smsAuthCache.RemoveAsync(phoneNumber);
            await smsAuthCache.RemoveAsync(retryKey);
            return true;
        }

        private Task<string> GenerateSmsAuthCode()
        {
            int randomCode = _random.Next(100000, 999999);
            return Task.FromResult(randomCode.ToString());
        }
    }
}
