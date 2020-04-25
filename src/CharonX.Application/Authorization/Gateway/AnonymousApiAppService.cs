using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Reflection.Extensions;
using CharonX.Authorization.Gateway.Dto;
using CharonX.Configuration;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace CharonX.Authorization.Gateway
{
    public class AnonymousApiAppService : IAnonymousApiAppService
    {
        private const string GetServiceUrlPart = @"{0}/services/";

        private readonly IConfigurationRoot _configurationRoot;
        private readonly string _address;
        private readonly string _getServiceUrlPattern;

        private string _serviceUrl;
        private string _jwtUrl;
        private string _jwtAnonymousUrl;


        public AnonymousApiAppService()
        {
            _configurationRoot = AppConfigurations.Get(
                typeof(CharonXApplicationModule).GetAssembly().GetDirectoryPathOrNull());

            _address = _configurationRoot["Gateway:Address"].Trim('/');

            _getServiceUrlPattern = string.Format(GetServiceUrlPart, _address) + "{0}";
        }

        public async Task AddAnonymousRouteAsync(SetAnonymousDto setAnonymousDto)
        {
            if ((setAnonymousDto.Urls == null) || (!CheckServiceExistence(setAnonymousDto.ServiceName)))
            {
                return;
            }

            string existAnonymous = GetJwtIdAndAnonymous(out var jwtId);

            var newAnonymous = GenerateNewAnonymous(existAnonymous, setAnonymousDto.Urls, true);

            SetJwtAnonymous(jwtId, newAnonymous);
        }

        private bool CheckServiceExistence(string service)
        {
            _serviceUrl = string.Format(_getServiceUrlPattern, service);

            var client = new RestClient(_serviceUrl);
            var response = client.Get(new RestRequest());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            //var result = JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content);

            return true;
        }

        private string GetJwtIdAndAnonymous(out string jwtId)
        {
            _jwtUrl = _serviceUrl.Trim('/') + "/plugins";

            var client = new RestClient(_jwtUrl);
            IRestRequest request = new RestRequest();
            request.AddParameter("name", "jwt");
            var response = client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                jwtId = string.Empty;
                return string.Empty;
            }

            string temp = string.Empty;
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content);
            temp = result["data"].ToString();
            var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(temp); ;
            temp = data[0]["config"].ToString();
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(temp);

            jwtId = data[0]["id"].ToString();

            return config["anonymous"] != null ? config["anonymous"].ToString() : string.Empty;
        }

        private static string GenerateNewAnonymous(string existAnonymous, List<string> urls, bool addMode)
        {
            List<string> existAnonymousList = existAnonymous.Split(",").ToList();

            List<string> newAnonymousList = null;
            if (addMode)
            {
                newAnonymousList = existAnonymousList.Union(urls).ToList();
            }
            else
            {
                newAnonymousList = existAnonymousList.Except(urls).ToList();
            }

            StringBuilder sb = new StringBuilder();
            foreach (string url in newAnonymousList)
            {
                sb.Append(",");
                sb.Append(url);
            }

            return sb.ToString();
        }

        private void SetJwtAnonymous(string jwtId, string newAnonymous)
        {
            _jwtAnonymousUrl = _jwtUrl.Trim('/') + "/" + jwtId;
            var client = new RestClient(_jwtAnonymousUrl);
            IRestRequest request = new RestRequest();

            newAnonymous = newAnonymous.Trim(',');
            string content = "config.anonymous=" + newAnonymous;

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", content, ParameterType.RequestBody);

            var response = client.Patch(request);
        }

        public async Task RemoveAnonymousRouteAsync(SetAnonymousDto setAnonymousDto)
        {
            if ((setAnonymousDto.Urls == null) || (!CheckServiceExistence(setAnonymousDto.ServiceName)))
            {
                return;
            }

            string existAnonymous = GetJwtIdAndAnonymous(out var jwtId);

            var newAnonymous = GenerateNewAnonymous(existAnonymous, setAnonymousDto.Urls, false);

            SetJwtAnonymous(jwtId, newAnonymous);
        }

        public async Task ResetAnonymousRouteAsync(ResetAnonymousDto resetAnonymousDto)
        {
            if (!CheckServiceExistence(resetAnonymousDto.ServiceName))
            {
                return;
            }

            string existAnonymous = GetJwtIdAndAnonymous(out var jwtId);
            SetJwtAnonymous(jwtId, string.Empty);
        }
    }
}
