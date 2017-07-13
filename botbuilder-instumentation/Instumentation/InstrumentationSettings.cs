using System.Collections.Generic;
using BotBuilder.Instrumentation.Managers;

namespace BotBuilder.Instrumentation.Instumentation
{
    public class InstrumentationSettings
    {
        public List<string> InstrumentationKeys { get; set; }
        public SentimentManager SentimentManager { get; set; }
        public bool OmitUsernameFromTelemetry { get; set; }
    }
}
