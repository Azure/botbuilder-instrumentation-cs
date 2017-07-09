using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO: Ultimatively this should be into a separate assembly
namespace BotBuilder.Instrumentation.Interfaces
{
    public interface IBotFrameworkInstrumentation
    {
        Task TrackActivity(IActivity activity, IBotData botData = null, IDictionary<string, string> customProperties = null);
        void TrackLuisIntent(IActivity activity, LuisResult result);
        void TrackQnaEvent(IActivity activity, string userQuery, string kbQuestion, string kbAnswer, double score);
        void TrackCustomEvent(IActivity activity, IDictionary<string, string> customEventProperties);
    }
}
