using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using BotBuilder.Instrumentation.Interfaces;
using Autofac;
using Microsoft.Bot.Builder.Luis;
using System.Configuration;

namespace BotBuilder.Instrumentation.Dialogs
{
    [Serializable]
    public class InstrumentedLuisDialog<TResult> : LuisDialog<TResult>
    {
        public InstrumentedLuisDialog(string luisModelId, string luisSubscriptionKey) : base(new LuisService(new LuisModelAttribute(luisModelId, luisSubscriptionKey)))
        {
        }

        public InstrumentedLuisDialog(string luisModelId, string luisSubscriptionKey, string domain) : base(new LuisService(new LuisModelAttribute(luisModelId, luisSubscriptionKey, LuisApiVersion.V2, domain)))
        {
        }

        protected override Task DispatchToIntentHandler(IDialogContext context, IAwaitable<IMessageActivity> item, IntentRecommendation bestIntent, LuisResult result)
        {
            using (var scope = Conversation.Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IBotFrameworkInstrumentation>();
                service.TrackLuisIntent(context.Activity, result);
            }
            return base.DispatchToIntentHandler(context, item, bestIntent, result);
        }
    }
}
