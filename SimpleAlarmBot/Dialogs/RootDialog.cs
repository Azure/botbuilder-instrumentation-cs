using System;
using System.Threading.Tasks;
using Chronic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SimpleAlarmBot.Services;
using Microsoft.Bot.Sample.SimpleAlarmBot.Telemetry;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Dialogs
{
    [LuisModel("769815d3-074d-49fe-9907-2d6a0e203e82", "61b5f3fa57724702a3a16a045867ab39")]
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private const string _entityAlarmName = "AlarmName";
        private const string _entityAlarmStartTime = "builtin.datetime.time";
        private static readonly AlarmService _alarms = new AlarmService();

        protected override Task DispatchToIntentHandler(IDialogContext context, IAwaitable<IMessageActivity> item, IntentRecommendation bestInent, LuisResult result)
        {
            // Log the resolved intent. 
            TelemetryLogger.TrackLuisIntent(context.Activity, result);
            return base.DispatchToIntentHandler(context, item, bestInent, result);
        }

        [LuisIntent("alarm.set")]
        public async Task SetAlarm(IDialogContext context, LuisResult result)
        {
            try
            {
                var alarmForm = InitAlarmForm(result);
                var alarmFormDialog = new FormDialog<AlarmForm>(alarmForm, AlarmForm.BuildForm, FormOptions.PromptInStart);
                context.Call(alarmFormDialog, ResumeAfterAlarmSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [LuisIntent("alarm.delete")]
        public async Task DeleteAlarm(IDialogContext context, LuisResult result)
        {
            EntityRecommendation title;
            result.TryFindEntity(_entityAlarmName, out title);

            if (title != null)
            {
                await _alarms.DeleteAlarm(context, title.Entity);
                context.Wait(MessageReceived);
            }
            else
            {
                var dialog = new PromptDialog.PromptChoice<string>(_alarms.Alarms, "Which alarm would you like to delete?", "Didn't get that!", 3);
                context.Call(dialog, ResumeAfterAlarmDelete);
            }
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I\'m sorry I didn\'t understand. I can only create & delete alarms.");
            context.Wait(MessageReceived);
        }

        private static async Task ResumeAfterAlarmSet(IDialogContext context, IAwaitable<AlarmForm> result)
        {
            try
            {
                var alarm = await result;
                _alarms.CreateAlarm(context.Activity.AsMessageActivity(), alarm);
            }
            catch (FormCanceledException ex)
            {
                var reply = ex.InnerException == null ? "operation cancelled" : $"Error occurred:{ex.InnerException.Message}";
                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private static async Task ResumeAfterAlarmDelete(IDialogContext context, IAwaitable<string> result)
        {
            var alarmNam = await result;
            await _alarms.DeleteAlarm(context, alarmNam);
            context.Done(true);
        }

        /// <summary>
        /// Try to get alarm entity info from LUIS result and initialize the form
        /// </summary>
        private static AlarmForm InitAlarmForm(LuisResult result)
        {
            EntityRecommendation title;
            result.TryFindEntity(_entityAlarmName, out title);

            EntityRecommendation time;
            result.TryFindEntity(_entityAlarmStartTime, out time);
            DateTime? when = null;
            if (time != null)
            {
                var parser = new Parser();
                var span = parser.Parse(time.Entity);
                when = span.Start ?? span.End;
            }

            return new AlarmForm
            {
                Name = title?.Entity,
                Time = when
            };
        }
    }
}