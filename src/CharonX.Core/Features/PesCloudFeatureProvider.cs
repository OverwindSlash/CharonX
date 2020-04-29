using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Features;
using Abp.Localization;
using Abp.UI.Inputs;

namespace CharonX.Features
{
    public class PesCloudFeatureProvider : FeatureProvider
    {
        public static string SmartSecurityFeature = "SmartSecurityFeature";
        public static string SmartPassFeature = "SmartPassFeature";
        public static string SourceName = CharonXConsts.LocalizationSourceName;

        private readonly ILocalizationManager _localizationManager;

        public PesCloudFeatureProvider(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }

        public override void SetFeatures(IFeatureDefinitionContext context)
        {
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
        }
    }
}
