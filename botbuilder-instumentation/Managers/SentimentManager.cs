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
        private readonly IDictionary<string, string> _httpHeaders;
        private readonly IHttpCommunication _httpCommunication;

        private const string SentimentApiRoute = "text/analytics/v2.0/sentiment";
        private const string SubscriptionKey = "Ocp-Apim-Subscription-Key";

        public SentimentManager(string textAnalyiticsApiKey, string textAnalyticsMinLength,
            string cognitiveServiceApiEndpoint, IHttpCommunication httpCommunication = null)
        {
            _textAnalyticsApiKey = textAnalyiticsApiKey;
            if (!string.IsNullOrWhiteSpace(_textAnalyticsApiKey))
            {
                _httpHeaders = new Dictionary<string, string> {{SubscriptionKey, _textAnalyticsApiKey}};
            }

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
            var sentimentInfo = await GetSentiment(jsonSentimentInput);
            return sentimentInfo.Documents[0].Score;
        }

        private async Task<BatchResult> GetSentiment(string jsonSentimentInput)
        {
            var data = Encoding.UTF8.GetBytes(jsonSentimentInput);

            var sentimentRawResponse = await _httpCommunication.PostAsync(_cognitiveServiceApiEndpoint, 
                SentimentApiRoute, _httpHeaders, data);

            return JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
        }
    }
}
