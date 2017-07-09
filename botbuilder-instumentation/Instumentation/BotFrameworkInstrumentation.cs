using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using BotBuilder.Instrumentation.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Autofac;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights.DataContracts;
using BotBuilder.Instrumentation.Telemetry;
using BotBuilder.Instrumentation.Managers;
using BotBuilder.Instrumentation.Instumentation;

namespace BotBuilder.Instrumentation
{
    public class BotFrameworkApplicationInsightsInstrumentation : IBotFrameworkInstrumentation
    {
        private List<TelemetryClient> _telemetryClients;
        private InstrumentationSettings _settings;

        public BotFrameworkApplicationInsightsInstrumentation(InstrumentationSettings settings)
        {
            if(settings == null)
            {
                throw new System.Exception("Settings cannot be null");
            }
            if (settings.InstrumentationKeys == null || settings.InstrumentationKeys.Count==0)
            {
                throw new System.Exception("Settings must contain at least one instrumentation key");
            }
            _settings = settings;

            //init clients
            _telemetryClients = new List<TelemetryClient>();
            _settings.InstrumentationKeys.ForEach((key) => {
                _telemetryClients.Add(new TelemetryClient(new TelemetryConfiguration(key)));
            });
            
            // Register activity logger via autofac DI.
            var builder = new ContainerBuilder();
            builder.RegisterType<DialogActivityLogger>().As<IActivityLogger>().InstancePerLifetimeScope();
            builder.RegisterInstance<IBotFrameworkInstrumentation>(this).As<IBotFrameworkInstrumentation>().SingleInstance();
            builder.Update(Conversation.Container);
        }

        public async Task TrackActivity(IActivity activity, IBotData botData = null, IDictionary<string, string> customProperties = null)
        {
            var et = BuildEventTelemetry(activity, customProperties);

            _telemetryClients.ForEach(c => c.TrackEvent(et));

            // Track sentiment only for incoming messages. 
            if (et.Name == TelemetryEventTypes.MessageReceived)
            {
                await TrackMessageSentiment(activity, customProperties);
            }
        }

        public void TrackLuisIntent(IActivity activity, LuisResult result)
        {
            if (result == null || result.TopScoringIntent==null )
            {
                return;
            }
            var properties = new Dictionary<string, string>
            {
                {"intent", result.TopScoringIntent.Intent},
                {"score", result.TopScoringIntent.Score.ToString()},
                {"entities", JsonConvert.SerializeObject(result.Entities)}
            };

            var eventTelemetry = BuildEventTelemetry(activity, properties);
            eventTelemetry.Name = TelemetryEventTypes.LuisIntentDialog;
            _telemetryClients.ForEach(c => c.TrackEvent(eventTelemetry));
        }

        public void TrackQnaEvent(IActivity activity, string userQuery, string kbQuestion, string kbAnswer, double score)
        {
            var properties = new Dictionary<string, string>
            {
                {"userQuery", userQuery},
                {"kbQuestion", kbQuestion},
                {"kbAnswer", kbAnswer},
                {"score", score.ToString()}
            };

            var eventTelemetry = BuildEventTelemetry(activity, properties);
            eventTelemetry.Name = TelemetryEventTypes.QnaEvent;
            _telemetryClients.ForEach(c => c.TrackEvent(eventTelemetry));
        }

        public void TrackCustomEvent(IActivity activity, Dictionary<string, string> customEventProperties)
        {
            var eventTelemetry = BuildEventTelemetry(activity, customEventProperties);
            eventTelemetry.Name = TelemetryEventTypes.CustomEvent;
            _telemetryClients.ForEach(c => c.TrackEvent(eventTelemetry));
        }

        private async Task TrackMessageSentiment(IActivity activity, IDictionary<string, string> customProperties = null)
        {
            if (_settings.SentimentManager == null)
            {
                return;
            }

            var properties = await _settings.SentimentManager.GetSentimentProperties(activity.AsMessageActivity().Text);
            if (properties != null)
            {
                //if there are custom properties, also add them.
                if(customProperties != null)
                {
                    foreach(var kvp in customProperties)
                    {
                        if(!properties.ContainsKey(kvp.Key))
                        {
                            properties.Add(kvp.Key, kvp.Value);
                        }
                    }
                }

                var et = BuildEventTelemetry(activity, properties);
                et.Name = TelemetryEventTypes.MessageSentiment;
                _telemetryClients.ForEach(c => c.TrackEvent(et));
            }
        }

        /// <summary>
        /// Helper method to create an EventTelemetry instance and populate common properties depending on the message type.
        /// </summary>
        private EventTelemetry BuildEventTelemetry(IActivity activity, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var et = new EventTelemetry();
            if (activity.Timestamp != null) et.Properties.Add("timestamp", Utils.GetDateTimeAsIso8601(activity.Timestamp.Value));
            et.Properties.Add("type", activity.Type);
            et.Properties.Add("channel", activity.ChannelId);

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    var messageActivity = activity.AsMessageActivity();
                    if (activity.ReplyToId == null)
                    {
                        et.Name = TelemetryEventTypes.MessageReceived;
                        et.Properties.Add("userId", activity.From.Id);
                        if (!_settings.OmitUsernameFromTelemetry)
                        {
                            et.Properties.Add("userName", activity.From.Name);
                        }
                    }
                    else
                    {
                        et.Name = TelemetryEventTypes.MessageSent;
                    }
                    et.Properties.Add("text", messageActivity.Text);
                    et.Properties.Add("conversationId", messageActivity.Conversation.Id);
                    break;
                case ActivityTypes.ConversationUpdate:
                    et.Name = TelemetryEventTypes.ConversationUpdate;
                    break;
                case ActivityTypes.EndOfConversation:
                    et.Name = TelemetryEventTypes.ConversationEnded;
                    break;
                default:
                    et.Name = TelemetryEventTypes.OtherActivity;
                    break;
            }

            // Add any other properties received.
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    et.Properties.Add(property);
                }
            }

            // Add any other metrics received.
            if (metrics != null)
            {
                foreach (var metric in metrics)
                {
                    et.Metrics.Add(metric);
                }
            }

            return et;
        }

        

        

        
    }
}
