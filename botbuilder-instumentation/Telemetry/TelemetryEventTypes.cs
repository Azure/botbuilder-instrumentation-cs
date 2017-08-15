using System;

namespace BotBuilder.Instrumentation.Telemetry
{
    /// <summary>
    /// Define event type names used by TelemtryLogger.
    /// </summary>
    public static class TelemetryEventTypes
    {
        public const string MessageReceived = "MBFEvent.UserMessage";
        public const string MessageSent = "MBFEvent.BotMessage";
        public const string LuisIntentDialog = "MBFEvent.Intent";
        public const string MessageSentiment = "MBFEvent.Sentiment";
        public const string ConvertionStarted = "MBFEvent.StartTransaction";
        public const string ConvertionEnded = "MBFEvent.EndTransactiond";
        public const string OtherActivity = "MBFEvent.Other";
        public const string ConversationUpdate = "MBFEvent.StartConversation";
        public const string ConversationEnded = "MBFEvent.EndConversation";
        public const string QnaEvent = "MBFEvent.QNAEvent";
        public const string CustomEvent = "MBFEvent.CustomEvent";
        public const string GoalTriggeredEvent = "MBFEvent.GoalEvent";
    }
}