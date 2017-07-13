using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Instumentation;
using BotBuilder.Instrumentation.Managers;
using BotBuilder.Instrumentation.Telemetry;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace BotBuilder.Instrumentation.Tests
{
    [TestClass]
    public class BotFrameworkApplicationInsightsInstrumentationTests
    {
        [TestMethod]
        public void Constructor_NullSettings_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new BotFrameworkApplicationInsightsInstrumentation(null));
        }

        [TestMethod]
        public void Constructor_SettingsWithoutInstrumentationKey_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new BotFrameworkApplicationInsightsInstrumentation(new InstrumentationSettings()));
        }

        [TestMethod]
        public void Constructor_SettingsWithEmptyInstrumentationKeyList_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>
            (
                () => new BotFrameworkApplicationInsightsInstrumentation(new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>()
                })
            );
        }

        [TestMethod]
        public void Constructor_NonEmptySettings_DoesNotThrowException()
        {
            new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] {"instrumentation key"}),
                    SentimentManager = new SentimentManager("", "", "")
                }
            );
        }

        private static IActivity CreateActivity(string messageText, string activityType)
        {
            var messageActivityMock = new Mock<IMessageActivity>();
            messageActivityMock.SetupGet(x => x.Text).Returns(messageText);
            messageActivityMock.SetupGet(x => x.Conversation).Returns
            (
                new ConversationAccount
                {
                    Id = "conversation id"
                }
            );

            var activityMock = new Mock<IActivity>();
            activityMock.SetupGet(x => x.Type).Returns(activityType);
            activityMock.Setup(x => x.AsMessageActivity()).Returns(messageActivityMock.Object);
            activityMock.SetupGet(x => x.From).Returns
            (
                new ChannelAccount
                {
                    Id = "from id",
                    Name = "user name"
                }
            );

            return activityMock.Object;
        }

        [TestMethod]
        public async Task TrackActivity_NonIncomingMessage_DoesNotTrackMessageSentiment()
        {
            // Arrange
            var sentimentHttpMock = TestUtils.CreateSentimentHttpMock(50);
            var instrumentation = new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] {"instrumentation key"}),
                    SentimentManager = new SentimentManager("text analytics api key", "", "", sentimentHttpMock.Object)
                }
            );

            // Act
            await instrumentation.TrackActivity(CreateActivity("some text", ActivityTypes.EndOfConversation));

            // Assert if sentiment analysis endpoint was never called
            sentimentHttpMock.Verify(
                x =>
                    x.PostAsync
                    (
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Dictionary<string, string>>(),
                        It.IsAny<byte[]>()
                    ),
                Times.Never());
        }

        [TestMethod]
        public async Task TrackActivity_IncomingMessage_TracksMessageSentiment()
        {
            // Arrange
            const string textAnalyticsApiKey = "text analytics api key";
            const string cognitiveServiceApiEndpoint = "cognitive service api endpoint";
            const string messageText = "message text";

            var sentimentHttpMock = TestUtils.CreateSentimentHttpMock(50);
            var instrumentation = new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] {"instrumentation key"}),
                    SentimentManager = new SentimentManager(textAnalyticsApiKey, "",
                        cognitiveServiceApiEndpoint, sentimentHttpMock.Object)
                }
            );

            // Act
            await instrumentation.TrackActivity(CreateActivity(messageText, ActivityTypes.Message));

            // Assert if sentiment analysis endpoint was called with correct params
            sentimentHttpMock.Verify(
                x =>
                    x.PostAsync
                    (
                        It.Is<string>(s => s == cognitiveServiceApiEndpoint),
                        It.IsAny<string>(), 
                        It.Is<Dictionary<string, string>>(d => d.ContainsValue(textAnalyticsApiKey)),
                        It.Is<byte[]>(
                            b => JsonConvert.DeserializeObject<BatchInput>(Encoding.UTF8.GetString(b)).Documents[0]
                                     .Text == messageText)
                    ),
                Times.Once());
        }

        [TestMethod]
        public void TrackLuisIntent_SmokeTest()
        {
            var instrumentation = new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] {"instrumentation key"})
                }
            );

            var luisResult = new LuisResult
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

            instrumentation.TrackLuisIntent(CreateActivity("some text", ActivityTypes.Message), luisResult);
        }

        [TestMethod]
        public void TrackQnaEvent_SmokeTest()
        {
            var instrumentation = new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] { "instrumentation key" })
                }
            );

            instrumentation.TrackQnaEvent(CreateActivity("some text", ActivityTypes.Message), "user query",
                "kb question", "kb answer", 90);
        }

        [TestMethod]
        public void TrackCustomEvent_SmokeTest()
        {
            var instrumentation = new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] { "instrumentation key" })
                }
            );

            var customProperties = new Dictionary<string, string>();
            for (var i = 0; i < 10; i++)
                customProperties.Add("key" + i, "value " + i);

            instrumentation.TrackCustomEvent(CreateActivity("some text", ActivityTypes.Message), 
                customEventProperties: customProperties);
        }
    }
}