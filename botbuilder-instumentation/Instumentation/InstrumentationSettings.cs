using BotBuilder.Instrumentation.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotBuilder.Instrumentation.Instumentation
{
    public class InstrumentationSettings
    {
        public List<string> InstrumentationKeys { get; set; }
        public SentimentManager SentimentManager { get; set; }
        public bool OmitUsernameFromTelemetry { get; set; }
    }
}
