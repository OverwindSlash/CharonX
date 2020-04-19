using Abp;
using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.UI;
using CharonX.Authorization.Users;
using CharonX.Editions;
using System;
using System.Threading.Tasks;

namespace CharonX.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore)
        {
        }

        public async Task<Tenant> GetAvailableTenantById(int tenantId)
        {
            Tenant tenant = null;

            try
            {
                tenant = await GetByIdAsync(tenantId);
            }
            catch (AbpException abpException)
            {
                throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId), abpException);
            }

            if (!tenant.IsActive)
            {
                throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
            }

            return tenant;
        }

        protected string L(string name, params object[] args)
        {
            LocalizableString localizableString = new LocalizableString(name, CharonXConsts.LocalizationSourceName);
            string pattern = LocalizationManager.GetString(localizableString);
            return String.Format(pattern, args);
        }
    }
}
