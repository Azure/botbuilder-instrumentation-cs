using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BotBuilder.Instrumentation.Interfaces;

namespace BotBuilder.Instrumentation
{
    public class HttpCommunication : IHttpCommunication
    {
        private const string JsonMimeType = "application/json";

        /// <summary>
        /// In case of Http exception, the method returns null
        /// </summary>
        /// <param name="baseEndpoint"></param>
        /// <param name="route"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string baseEndpoint, string route, IDictionary<string, string> headers, byte[] data)
        {
            if (!Uri.TryCreate(baseEndpoint, UriKind.Absolute, out Uri baseUri))
                throw new ArgumentException("baseEndpoint should be a valid uri");

            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = baseUri;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                
                using (var content = new ByteArrayContent(data))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(JsonMimeType);

                    try
                    {
                        var response = await httpClient.PostAsync(route, content);
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                    catch (HttpRequestException e)
                    {
                        Trace.WriteLine(e);
                        return null;
                    }
                }
            }
        }
    }
}
