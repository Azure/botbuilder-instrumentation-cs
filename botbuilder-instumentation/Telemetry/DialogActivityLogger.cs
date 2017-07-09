using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using BotBuilder.Instrumentation.Interfaces;
using Autofac;

namespace BotBuilder.Instrumentation.Telemetry
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
            //TODO: Check if there is apannelty for creating this "scope" again and again
            using (var scope = Conversation.Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IBotFrameworkInstrumentation>();
                await service.TrackActivity(activity, _botData);
            }
        }
    }
}