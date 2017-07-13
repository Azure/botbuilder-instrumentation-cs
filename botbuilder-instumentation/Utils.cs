using Newtonsoft.Json;
using System;

namespace BotBuilder.Instrumentation
{
    public class Utils
    {
        internal static string GetDateTimeAsIso8601(DateTime activity)
        {
            var s = JsonConvert.SerializeObject(activity.ToUniversalTime());
            return s.Substring(1, s.Length - 2);
        }
    }
}
