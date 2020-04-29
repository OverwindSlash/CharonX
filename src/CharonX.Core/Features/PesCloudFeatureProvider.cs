using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.Application.Features;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Abp.UI.Inputs;
using CharonX.Authorization.Users;
using CharonX.Entities;
using CharonX.MultiTenancy;

namespace CharonX.Features
{
    public class PesCloudFeatureProvider : FeatureProvider
    {
        public static string SmartSecurityFeature = "SmartSecurityFeature";
        public static string SmartPassFeature = "SmartPassFeature";
        public static string SourceName = CharonXConsts.LocalizationSourceName;

        private readonly ILocalizationManager _localizationManager;
        private readonly IRepository<CustomFeatureSetting> featureRepository;

        public PesCloudFeatureProvider(
            IRepository<CustomFeatureSetting> featureRepository
            )
        {
            //_localizationManager = localizationManager;
            this.featureRepository = featureRepository;
        }

        public override void SetFeatures(IFeatureDefinitionContext context)
        {
#if true
            var smartSecurityFeature = context.Create(
                SmartSecurityFeature,
                displayName: new LocalizableString(SmartSecurityFeature, SourceName),
                defaultValue: "false",
                inputType: new CheckboxInputType()
            );

            var smartPassFeature = context.Create(
                SmartPassFeature,
                displayName: new LocalizableString(SmartPassFeature, SourceName),
                defaultValue: "false",
                inputType: new CheckboxInputType()
            );
#endif
            //additional features
            var features = GetAllFeatures();
            foreach (var feature in features)
            {
                context.Create(
                    feature,
                    displayName: new LocalizableString(feature, SourceName),
                    defaultValue: "false",
                    inputType: new CheckboxInputType());
            }
        }

        private List<string> GetAllFeatures()
        {
            List<string> featureStringList;
            var features = featureRepository.GetAllList();
            featureStringList = features
                .Select(feature => feature.Name).ToList();
            return featureStringList;
        }
    }
}
