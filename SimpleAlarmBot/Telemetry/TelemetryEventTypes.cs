using System;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry
{
    /// <summary>
    /// Define event type names used by TelemtryLogger.
    /// </summary>
    public static class TelemetryEventTypes
    {
        public const string MessageReceived = "message.received";
        public const string MessageSent = "message.send";
        public const string LuisIntentDialog = "message.intent.dialog";
        public const string MessageSentiment = "message.sentiment";
        public const string ConvertionStarted = "message.convert.start";
        public const string ConvertionEnded = "message.convert.end";
        public const string OtherActivity = "message.other";
        public const string ConversationUpdate = "message.conversation.updated";
        public const string ConversationEnded = "message.conversation.ended";
    }
}