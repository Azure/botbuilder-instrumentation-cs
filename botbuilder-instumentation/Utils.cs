using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotBuilder.Instrumentation
{
    public class Utils
    {
        internal static string GetDateTimeAsIso8601(DateTime activity)
        {
            var s = JsonConvert.SerializeObject(activity.ToUniversalTime());
            return s.Substring(1, s.Length - 2);
        }

        internal static async Task<string> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
