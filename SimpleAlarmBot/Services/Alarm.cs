using System;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Services
{
    /// <summary>
    /// Helper class to store alarm info and fire alarms when the time comes.
    /// </summary>
    public sealed class Alarm : IDisposable
    {
        private readonly string _resumptionCookie;
        private Timer _alarmTimer;

        public Alarm(string name, DateTime time, string resumptionCookie)
        {
            _resumptionCookie = resumptionCookie;
            Name = name;
            When = time;
        }

        public string Name { get; }
        public DateTime When { get; }

        public void Dispose()
        {
            _alarmTimer?.Dispose();
            _alarmTimer = null;
        }

        /// <summary>
        /// Sets a timer to fire off the alarm. 
        /// </summary>
        public void Enable()
        {
            _alarmTimer = new Timer(FireAlarm);
            _alarmTimer.Change((long)When.Subtract(DateTime.Now).TotalMilliseconds, Timeout.Infinite);
        }

        public override string ToString()
        {
            return $"[{Name} at {When}]";
        }

        private void FireAlarm(object state)
        {
            var message = ResumptionCookie.GZipDeserialize(_resumptionCookie).GetMessage();
            var client = new ConnectorClient(new Uri(message.ServiceUrl));
            var reply = message.CreateReply();
            reply.Text = $"Hello, this is the {this} firing.";

            // We don't have a dialog so we need to log the message manually.
            TelemetryLogger.TrackActivity(reply).Wait(CancellationToken.None);
            client.Conversations.ReplyToActivityAsync(reply);

            _alarmTimer.Dispose();
        }
    }
}