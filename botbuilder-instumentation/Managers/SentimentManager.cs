using BotBuilder.Instrumentation.Telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotBuilder.Instrumentation.Managers
{
    public class SentimentManager
    {
        private string _textAnalyticsApiKey;
        private string _cognitiveServiceApiEndpoint;
        private int _textAnalyticsMinLength;

        public SentimentManager(string textAnalyiticsApiKey,string textAnalyticsMinLength, string cognitiveServiceApiEndpoint)
        {
            _textAnalyticsApiKey = textAnalyiticsApiKey;
            _cognitiveServiceApiEndpoint = cognitiveServiceApiEndpoint;
            if(!Int32.TryParse(textAnalyticsMinLength,out _textAnalyticsMinLength))
            {
                _textAnalyticsMinLength = 0;
            }
        }

        /// <summary>
        /// Helper method to track the sentiment of incoming messages.
        /// </summary>
        internal async Task<Dictionary<string, string>> GetSentimentProperties(string text)
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
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_cognitiveServiceApiEndpoint);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var byteData = Encoding.UTF8.GetBytes(jsonSentimentInput);
                var sentimentRawResponse = await Utils.CallEndpoint(client, "text/analytics/v2.0/sentiment", byteData);
                return JsonConvert.DeserializeObject<BatchResult>(sentimentRawResponse);
            }
        }
    }
}
