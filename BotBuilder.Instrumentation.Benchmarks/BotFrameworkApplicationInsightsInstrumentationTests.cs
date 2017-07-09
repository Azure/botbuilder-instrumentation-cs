using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BotBuilder.Instrumentation.Managers;
using BotBuilder.Instrumentation.Telemetry;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Moq;

namespace BotBuilder.Instrumentation.Benchmarks
{
    [MinColumn, MaxColumn]
    public class BotFrameworkApplicationInsightsInstrumentationTests
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
                    new Instumentation.InstrumentationSettings
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
            var sentimentManagerMock = new Mock<SentimentManager>("text analytics api key", "", "http://localhost");
            sentimentManagerMock.Setup(x => x.GetSentiment(It.IsAny<string>(), It.IsAny<string>())).Returns
            (
                Task.FromResult(new BatchResult
                {
                    Documents = new List<DocumentResult> {new DocumentResult {Score = 80}}
                })
            );

            return sentimentManagerMock.Object;
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
                    Score = 60
                },
                Entities = new List<EntityRecommendation>
                {
                    new EntityRecommendation
                    {
                        Entity = "luis entity",
                        Type = "entity type",
                        Score = 70,
                        Role = "entity role"
                    }
                }
            };
        }

        #endregion

        #region Benchmarks

        [Benchmark]
        public async Task TestTrackActivityAsync()
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
            _defaultInstrumentation.TrackCustomEvent(_activity, _customProperties);
        }

        #endregion
    }
}
