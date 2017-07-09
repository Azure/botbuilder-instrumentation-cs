using BotBuilder.Instrumentation.Managers;
using System.Collections.Generic;

namespace BotBuilder.Instrumentation.Instumentation
{
    public class InstrumentationSettings
    {
        public List<string> InstrumentationKeys { get; set; }
        public SentimentManager SentimentManager { get; set; }
        public bool OmitUsernameFromTelemetry { get; set; }
    }
}
