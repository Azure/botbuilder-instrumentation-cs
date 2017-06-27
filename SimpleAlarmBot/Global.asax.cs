using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Initialize telemetry subsytem.
            TelemetryLogger.Initialize(ConfigurationManager.AppSettings["InstrumentationKey"], ConfigurationManager.AppSettings["TextAnalyticsApiKey"], ConfigurationManager.AppSettings["TextAnalyticsMinLenght"]);

            // Configure Web API.
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}