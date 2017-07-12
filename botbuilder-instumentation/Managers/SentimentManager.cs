using BotBuilder.Instrumentation.Telemetry;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Interfaces;

namespace BotBuilder.Instrumentation.Managers
{
    public class SentimentManager
    {
        private readonly string _textAnalyticsApiKey;
        private readonly string _cognitiveServiceApiEndpoint;
        private readonly int _textAnalyticsMinLength;
        private readonly IHttpCommunication _httpCommunication;

        public SentimentManager(string textAnalyiticsApiKey, string textAnalyticsMinLength,
            string cognitiveServiceApiEndpoint, IHttpCommunication httpCommunication = null)
        {
            _textAnalyticsApiKey = textAnalyiticsApiKey;
            _cognitiveServiceApiEndpoint = cognitiveServiceApiEndpoint;
            if (!int.TryParse(textAnalyticsMinLength, out _textAnalyticsMinLength))
            {
                _textAnalyticsMinLength = 0;
            }
            _httpCommunication = httpCommunication ?? new HttpCommunication();
        }

        /// <summary>
        /// Helper method to track the sentiment of incoming messages.
        /// </summary>
        public async Task<Dictionary<string, string>> GetSentimentProperties(string text)
        {
            if (string.IsNullOrWhiteSpace(_textAnalyticsApiKey))
            {
                return null;
            }

            var numWords = text.Split(' ').Length;
            if (numWords >= _textAnalyticsMinLength)
            {
                return new Dictionary<string, string>
                {
                    {"score", (await GetSentimentScore(text)).ToString(CultureInfo.InvariantCulture)}
                };
            }
            return null;
        }

        private async Task<double> GetSentimentScore(string message)
        {
            var docs = new List<DocumentInput>
            {
                new DocumentInput {Id = 1, Text = message}
            };
            var sentimentInput = new BatchInput { Documents = docs };
            var jsonSentimentInput = JsonConvert.SerializeObject(sentimentInput);
            var sentimentInfo = await GetSentiment(_textAnalyticsApiKey, jsonSentimentInput);
            return sentimentInfo.Documents[0].Score;
        }

        private async Task<BatchResult> GetSentiment(string apiKey, string jsonSentimentInput)
        {
            const string sentimentRoute = "text/analytics/v2.0/sentiment";
            var headers = new Dictionary<string, string> {{"Ocp-Apim-Subscription-Key", apiKey}};
            var data = Encoding.UTF8.GetBytes(jsonSentimentInput);

            var sentimentRawResponse = await _httpCommunication.SendAsync(_cognitiveServiceApiEndpoint, 
                sentimentRoute, headers, data);

            return JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
        }
    }
}
