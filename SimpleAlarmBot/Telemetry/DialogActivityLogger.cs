using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry
{
    /// <summary>
    /// A generic logger for Dialog activities. 
    /// </summary>
    public class DialogActivityLogger : IActivityLogger
    {
        private readonly IBotData _botData;

        public DialogActivityLogger(IBotData botData)
        {
            _botData = botData;
        }

        public async Task LogAsync(IActivity activity)
        {
            await TelemetryLogger.TrackActivity(activity, _botData);
        }
    }
}