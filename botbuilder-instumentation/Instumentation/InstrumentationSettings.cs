using System.Collections.Generic;
using BotBuilder.Instrumentation.Interfaces;

namespace BotBuilder.Instrumentation.Instumentation
{
    public class InstrumentationSettings
    {
        public List<string> InstrumentationKeys { get; set; }
        public ISentimentManager SentimentManager { get; set; }
        public bool OmitUsernameFromTelemetry { get; set; }
    }
}
