using System.Collections.Generic;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Interfaces;
using BotBuilder.Instrumentation.Telemetry;
using Moq;
using Newtonsoft.Json;

namespace BotBuilder.Instrumentation.Tests
{
    public static class TestUtils
    {
        public static Mock<IHttpCommunication> CreateSentimentHttpMock(double score)
        {
            var fakeSentimentResult = new BatchResult
            {
                Documents = new List<DocumentResult> { new DocumentResult { Score = score } }
            };

            var httpCommunicationMock = new Mock<IHttpCommunication>();
            httpCommunicationMock.Setup
            (
                x =>
                    x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<byte[]>()))
                        .Returns(Task.FromResult(JsonConvert.SerializeObject(fakeSentimentResult))
            );

            return httpCommunicationMock;
        }
    }
}
