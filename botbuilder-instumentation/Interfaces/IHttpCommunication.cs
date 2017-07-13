using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotBuilder.Instrumentation.Interfaces
{
    public interface IHttpCommunication
    {
        Task<string> PostAsync(string baseEndpoint, string route, IDictionary<string, string> headers, byte[] data);
    }
}