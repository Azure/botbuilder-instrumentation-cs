using System;
using System.Collections.Generic;
using BotBuilder.Instrumentation.Managers;
using System.Configuration;
using BotBuilder.Instrumentation.Instumentation;

namespace BotBuilder.Instrumentation
{
    public class DependencyResolver
    {
        private static DependencyResolver _dependencyResolver;

        private DependencyResolver() { }

        public static DependencyResolver Current => _dependencyResolver ?? (_dependencyResolver = new DependencyResolver());

        public InstrumentationSettings CreateBasicSettings()
        {
            return new InstrumentationSettings
            {
                InstrumentationKeys = new List<string>(new[] { ConfigurationManager.AppSettings["InstrumentationKey"] }),
                OmitUsernameFromTelemetry =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["InstrumentationShouldOmitUsernameFromTelemetry"])
            };
        }

        public SentimentManager CreateDefaultSentimentManager()
        {
            return new SentimentManager(
                ConfigurationManager.AppSettings["TextAnalyticsApiKey"],
                ConfigurationManager.AppSettings["TextAnalyticsMinLength"],
                ConfigurationManager.AppSettings["CognitiveServiceApiEndpoint"]
            );
        }

        public InstrumentationSettings CreateSettingsWithCognitiveServices()
        {
            var settings = CreateBasicSettings();
            settings.SentimentManager = CreateDefaultSentimentManager();
            return settings;
        }

        public BotFrameworkApplicationInsightsInstrumentation CreateBasicInstrumentation()
        {
            return new BotFrameworkApplicationInsightsInstrumentation(CreateBasicSettings());
        }

        public BotFrameworkApplicationInsightsInstrumentation CreateInstrumentationWithCognitiveServices()
        {
            return new BotFrameworkApplicationInsightsInstrumentation(CreateSettingsWithCognitiveServices());
        }
    }
}
