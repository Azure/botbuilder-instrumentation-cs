using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotBuilder.Instrumentation.Interfaces
{
    public interface ISentimentManager
    {
        Task<Dictionary<string, string>> GetSentimentProperties(string text);
    }
}