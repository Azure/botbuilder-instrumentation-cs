using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Ultimatively this should be separated into a different assembly
/// </summary>
namespace BotBuilder.Instrumentation.Interfaces
{
    public interface IBotFrameworkInstrumentation
    {
        Task TrackActivity(IActivity activity, IBotData botData = null, IDictionary<string, string> customProperties = null);
        void TrackLuisIntent(IActivity activity, LuisResult result);
        void TrackQnaEvent(IActivity activity, string userQuery, string kbQuestion, string kbAnswer, double score);
        void TrackCustomEvent(IActivity activity, Dictionary<string, string> customEventProperties);

    }
}
