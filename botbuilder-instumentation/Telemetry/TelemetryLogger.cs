using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry
{
    /// <summary>
    /// Helper class to log events to AppInsights.
    /// </summary>
    public static class TelemetryLogger
    {
        private static string _textAnalyticsMinLength;
        private static string _textAnalyticsApiKey;

        /// <summary>
        /// Static telemetry client instance used to log AppInsights events.. 
        /// </summary>
        public static TelemetryClient TelemetryClient { get; } = new TelemetryClient();

        /// <summary>
        /// Initializes the telemtry subsystem.
        /// </summary>
        /// <param name="activeInstrumentationKey"></param>
        /// <param name="textAnalyticsApiKey"></param>
        /// <param name="textAnalyticsMinLength"></param>
        public static void Initialize(string activeInstrumentationKey, string textAnalyticsApiKey = null, string textAnalyticsMinLength = null)
        {
            // Initialize AppInsights with telemetry key.
            TelemetryConfiguration.Active.InstrumentationKey = activeInstrumentationKey;

            // Set the text analytics parameters.
            _textAnalyticsMinLength = textAnalyticsMinLength;
            _textAnalyticsApiKey = textAnalyticsApiKey;

            // Register activity logger
            var builder = new ContainerBuilder();
            builder.RegisterType<DialogActivityLogger>().As<IActivityLogger>().InstancePerLifetimeScope();
            builder.Update(Conversation.Container);
        }

        /// <summary>
        /// Logs an IActivity as a Custom Event to AppInishgts.
        /// </summary>
        public static async Task TrackActivity(IActivity activity, IBotData botData = null, IDictionary<string, string> customProperties = null)
        {
            var et = BuildEventTelemetry(activity, customProperties);
            
            TelemetryClient.TrackEvent(et);

            // Track sentiment only for incoming messages. 
            if (et.Name == TelemetryEventTypes.MessageReceived)
            {
                await TrackMessageSentiment(activity);
            }
        }

        /// <summary>
        /// Logs a LUIS intent to AppInisghts.
        /// </summary>
        public static void TrackLuisIntent(IActivity activity, LuisResult result)
        {
            var properties = new Dictionary<string, string>
            {
                {"intent", result.Intents[0].Intent},
                {"score", result.Intents[0].Score.ToString()},
                {"entities", JsonConvert.SerializeObject(result.Entities)}
            };

            var eventTelemetry = BuildEventTelemetry(activity, properties);
            eventTelemetry.Name = TelemetryEventTypes.LuisIntentDialog;
            TelemetryClient.TrackEvent(eventTelemetry);
        }

        public static void TrackQnaEvent(IActivity activity, string userQuery, string kbQuestion, string kbAnswer, double score)
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
            TelemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Helper method to track the sentiment of incoming messages.
        /// </summary>
        private static async Task TrackMessageSentiment(IActivity activity)
        {
            var text = activity.AsMessageActivity().Text;
            var numWords = text.Split(' ').Length;
            if (numWords >= Int32.Parse(_textAnalyticsMinLength) && _textAnalyticsApiKey != String.Empty)
            {
                var properties = new Dictionary<string, string>
                {
                    {"score", (await GetSentimentScore(text)).ToString(CultureInfo.InvariantCulture)}
                };

                var et = BuildEventTelemetry(activity, properties);
                et.Name = TelemetryEventTypes.MessageSentiment;
                TelemetryClient.TrackEvent(et);
            }
        }

        /// <summary>
        /// Helper method to create an EventTelemetry instance and populate common properties depending on the message type.
        /// </summary>
        private static EventTelemetry BuildEventTelemetry(IActivity activity, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var et = new EventTelemetry();
            if (activity.Timestamp != null) et.Properties.Add("timestamp", GetDateTimeAsIso8601(activity.Timestamp.Value));
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
                        et.Properties.Add("userName", activity.From.Name);
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

        private static string GetDateTimeAsIso8601(DateTime activity)
        {
            var s = JsonConvert.SerializeObject(activity.ToUniversalTime());
            return s.Substring(1, s.Length - 2);
        }

        private static async Task<double> GetSentimentScore(string message)
        {
            var docs = new List<DocumentInput>
            {
                new DocumentInput {Id = 1, Text = message}
            };
            var sentimentInput = new BatchInput {Documents = docs};
            var jsonSentimentInput = JsonConvert.SerializeObject(sentimentInput);
            var sentimentInfo = await GetSentiment(_textAnalyticsApiKey, jsonSentimentInput);
            return sentimentInfo.Documents[0].Score;
        }

        private static async Task<BatchResult> GetSentiment(string apiKey, string jsonSentimentInput)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://westus.api.cognitive.microsoft.com/");

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var byteData = Encoding.UTF8.GetBytes(jsonSentimentInput);
                var sentimentRawResponse = await CallEndpoint(client, "text/analytics/v2.0/sentiment", byteData);
                return JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
            }
        }

        private static async Task<string> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}