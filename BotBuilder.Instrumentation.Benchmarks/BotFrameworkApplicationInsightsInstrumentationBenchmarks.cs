using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BotBuilder.Instrumentation.Instumentation;
using BotBuilder.Instrumentation.Interfaces;
using BotBuilder.Instrumentation.Managers;
using BotBuilder.Instrumentation.Telemetry;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Moq;
using Newtonsoft.Json;

namespace BotBuilder.Instrumentation.Benchmarks
{
    [MinColumn, MaxColumn]
    [ShortRunJob]
    public class BotFrameworkApplicationInsightsInstrumentationBenchmarks
    {
        #region One time Setup

        private BotFrameworkApplicationInsightsInstrumentation _defaultInstrumentation;
        private IActivity _activity;
        private IDictionary<string, string> _customProperties;
        private LuisResult _luisResult;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _defaultInstrumentation =
                new BotFrameworkApplicationInsightsInstrumentation
                (
                    new InstrumentationSettings
                    {
                        InstrumentationKeys = new List<string>(new[] {"instrumentation key"}),
                        OmitUsernameFromTelemetry = false,
                        SentimentManager = GetSentimentManager()
                    }
                );

            SetupActivity();
            SetupCustomProperties();
            SetupLuisResult();
        }

        private static SentimentManager GetSentimentManager()
        {
            var fakeSentimentResult = new BatchResult
            {
                Documents = new List<DocumentResult> {new DocumentResult {Score = 60}}
            };

            var httpCommunicationMock = new Mock<IHttpCommunication>();
            httpCommunicationMock.Setup
            (
                x =>
                    x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<byte[]>()))
                        .Returns(Task.FromResult(JsonConvert.SerializeObject(fakeSentimentResult))
            );

            return new SentimentManager("text analytics api key", "", "http://localhost", httpCommunicationMock.Object);
        }

        private void SetupActivity()
        {
            var activityMock = new Mock<IActivity>();
            activityMock.SetupGet(x => x.Timestamp).Returns(DateTime.UtcNow);
            activityMock.SetupGet(x => x.Type).Returns(ActivityTypes.Message);
            activityMock.SetupGet(x => x.ChannelId).Returns("channel id");
            activityMock.SetupGet(x => x.From).Returns
            (
                new ChannelAccount
                {
                    Id = "from id",
                    Name = "user name"
                }
            );

            activityMock.Setup(x => x.AsMessageActivity()).Returns(GetMessageActivity());
            _activity = activityMock.Object;
        }

        private static IMessageActivity GetMessageActivity()
        {
            var messageActivityMock = new Mock<IMessageActivity>();
            messageActivityMock.SetupGet(x => x.Text).Returns("message text");
            messageActivityMock.SetupGet(x => x.Conversation).Returns
            (
                new ConversationAccount
                {
                    Id = "conversation id"
                }
            );

            return messageActivityMock.Object;
        }

        private void SetupCustomProperties()
        {
            _customProperties = new Dictionary<string, string>();
            for (var i = 0; i < 10; i++)
                _customProperties.Add("key" + i, "value " + i);
        }

        private void SetupLuisResult()
        {
            _luisResult = new LuisResult
            {
                TopScoringIntent = new IntentRecommendation
                {
                    Intent = "luis intent",
                    Score = 70
                },
                Entities = new List<EntityRecommendation>
                {
                    new EntityRecommendation
                    {
                        Entity = "luis entity",
                        Type = "entity type",
                        Score = 80,
                        Role = "entity role"
                    }
                }
            };
        }

        #endregion

        #region Benchmarks

        [Benchmark]
        public async Task TrackActivity()
        {
            await _defaultInstrumentation.TrackActivity(_activity, null, _customProperties);
        }

        [Benchmark]
        public void TrackLuisIntent()
        {
            _defaultInstrumentation.TrackLuisIntent(_activity, _luisResult);
        }

        [Benchmark]
        public void TrackQnaEvent()
        {
            _defaultInstrumentation.TrackQnaEvent(_activity, "user query", "kb question", "kb answer", 90);
        }

        [Benchmark]
        public void TrackCustomEvent()
        {
            _defaultInstrumentation.TrackCustomEvent(_activity, customEventProperties: _customProperties);
        }

        [Benchmark]
        public void TrackGoalTriggeredEvent()
        {
            _defaultInstrumentation.TrackGoalTriggeredEvent(_activity, "Goal name");
        }

        #endregion
    }
}
