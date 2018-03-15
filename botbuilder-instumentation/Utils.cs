using Newtonsoft.Json;
using System;

namespace BotBuilder.Instrumentation
{
    public class Utils
    {
        internal static string GetDateTimeOffsetAsIso8601(DateTimeOffset activity)
        {
            var s = JsonConvert.SerializeObject(activity.ToUniversalTime());
            return s.Substring(1, s.Length - 2);
        }
    }
}
