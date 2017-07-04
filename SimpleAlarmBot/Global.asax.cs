using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using LightInject;
using BotBuilder.Instrumentation;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //Setup DI
            IoC.Container.Register<BotBuilder.Instrumentation.Interfaces.IBotFrameworkInstrumentation>((service) => {
                return new BotFrameworkApplicationInsightsInstrumentation(
                    ConfigurationManager.AppSettings["InstrumentationKey"], 
                    new SentimentManager(
                            ConfigurationManager.AppSettings["TextAnalyticsApiKey"], 
                            ConfigurationManager.AppSettings["TextAnalyticsMinLenght"],
                            ConfigurationManager.AppSettings["CognitiveServiceApiEndpoint"]
                            ));
            }, new PerContainerLifetime()); //Only one telemetry instance is kept per application

            // Configure Web API.
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}