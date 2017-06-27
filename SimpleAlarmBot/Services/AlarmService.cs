using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleAlarmBot.Dialogs;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Services
{
    /// <summary>
    /// Mock service to store alarms.
    /// </summary>
    /// <remarks>
    /// In real life, we would store this somewhere, for this example, we just use a static dictionary and 
    /// timers to store and fire the alarms.
    /// </remarks>
    public class AlarmService
    {
        private readonly Dictionary<string, Alarm> _alarms = new Dictionary<string, Alarm>();

        /// <summary>
        /// Get a list of the alarms.
        /// </summary>
        public IList<string> Alarms => _alarms.Keys.ToList();

        /// <summary>
        /// Create an alarm.
        /// </summary>
        public void CreateAlarm(IMessageActivity message, AlarmForm alarmInfo)
        {
            if (alarmInfo.Time == null)
            {
                throw new ArgumentException("The alarm time can't be null.");
            }
            var resumptionCookie = new ResumptionCookie(message).GZipSerialize();
            var alarm = new Alarm(alarmInfo.Name, alarmInfo.Time.Value, resumptionCookie);
            _alarms.Add(alarmInfo.Name, alarm);
            alarm.Enable();
        }

        /// <summary>
        /// Delete an alarm.
        /// </summary>
        public async Task DeleteAlarm(IDialogContext context, string title)
        {
            _alarms.Remove(title);
            await context.PostAsync($"Deleted alarm {title}");
        }
    }
}