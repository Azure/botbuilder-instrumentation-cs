using System;
using System.Collections.Generic;
using BotBuilder.Instrumentation.Managers;
using System.Configuration;
using Autofac;
using BotBuilder.Instrumentation.Instumentation;
using BotBuilder.Instrumentation.Interfaces;

namespace BotBuilder.Instrumentation
{
    public class DependencyResolver
    {
        private static DependencyResolver _dependencyResolver;
        private readonly IContainer _container;

        public enum InstrumentationType
        {
            Basic,
            Cognitive
        }

        private DependencyResolver()
        {
            // Register instrumentation instances via Autofac DI.
            var builder = new ContainerBuilder();

            builder.Register(c => new BotFrameworkApplicationInsightsInstrumentation(CreateBasicSettings()))
                .Keyed<IBotFrameworkInstrumentation>(InstrumentationType.Basic)
                .SingleInstance();

            builder.Register(c => new BotFrameworkApplicationInsightsInstrumentation(CreateSettingsWithCognitiveServices()))
                .Keyed<IBotFrameworkInstrumentation>(InstrumentationType.Cognitive)
                .SingleInstance();

            _container = builder.Build();
        }

        public static DependencyResolver Current => _dependencyResolver ?? (_dependencyResolver = new DependencyResolver());

        public IBotFrameworkInstrumentation DefaultBasicInstrumentation
            => _container.ResolveKeyed<IBotFrameworkInstrumentation>(InstrumentationType.Basic);

        public IBotFrameworkInstrumentation DefaultInstrumentationWithCognitiveServices
            => _container.ResolveKeyed<IBotFrameworkInstrumentation>(InstrumentationType.Cognitive);

        private static InstrumentationSettings CreateBasicSettings()
        {
            return new InstrumentationSettings
            {
                InstrumentationKeys = new List<string>(new[] { ConfigurationManager.AppSettings["InstrumentationKey"] }),
                OmitUsernameFromTelemetry =
                    Convert.ToBoolean(ConfigurationManager.AppSettings["InstrumentationShouldOmitUsernameFromTelemetry"])
            };
        }

        private static InstrumentationSettings CreateSettingsWithCognitiveServices()
        {
            var settings = CreateBasicSettings();
            settings.SentimentManager = CreateDefaultSentimentManager();
            return settings;
        }

        private static SentimentManager CreateDefaultSentimentManager()
        {
            return new SentimentManager(
                ConfigurationManager.AppSettings["TextAnalyticsApiKey"],
                ConfigurationManager.AppSettings["TextAnalyticsMinLength"],
                ConfigurationManager.AppSettings["CognitiveServiceApiEndpoint"]
            );
        }
    }
}
