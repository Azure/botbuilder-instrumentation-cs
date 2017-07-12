using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Interfaces;

namespace BotBuilder.Instrumentation
{
    public class HttpCommunication : IHttpCommunication
    {
        private const string JsonMimeType = "application/json";

        public async Task<string> SendAsync(string baseEndpoint, string route, IDictionary<string, string> headers, byte[] data)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseEndpoint);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));

                foreach (var header in headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                
                using (var content = new ByteArrayContent(data))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(JsonMimeType);
                    var response = await httpClient.PostAsync(route, content);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
