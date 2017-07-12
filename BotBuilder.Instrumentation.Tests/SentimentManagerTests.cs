using System;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BotBuilder.Instrumentation.Tests
{
    [TestClass]
    public class SentimentManagerTests
    {
        [TestMethod]
        public void Constructor_NullArguments_DoesNotThrowException()
        {
            new SentimentManager(null, null, null);
        }

        [TestMethod]
        public void Constructor_EmptyArguments_DoesNotThrowException()
        {
            new SentimentManager("", "", "");
        }

        [TestMethod]
        public async Task GetSentimentProperties_EmptyApiKey_ReturnsNull()
        {
            var sentimentManager = new SentimentManager("", "", "");
            var result = await sentimentManager.GetSentimentProperties("analyze this text");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetSentimentProperties_TextWordsLessThanMinLength_ReturnsNull()
        {
            var sentimentManager = new SentimentManager("text analytics api key", "3", "");
            var result = await sentimentManager.GetSentimentProperties("hello");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetSentimentProperties_Text_ReturnsCorrectScore()
        {
            const double expectedScore = 60.0;
            var sentimentHttpMock = TestUtils.CreateSentimentHttpMock(expectedScore);

            var sentimentManager = new SentimentManager("text analytics api key", "2", "", sentimentHttpMock.Object);
            var result = await sentimentManager.GetSentimentProperties("hello world");

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedScore, Convert.ToDouble(result["score"]));
        }
    }
}
