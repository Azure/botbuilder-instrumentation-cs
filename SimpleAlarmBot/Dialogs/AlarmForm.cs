using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.SimpleAlarmBot.Dialogs
{
    /// <summary>
    /// A simple form to get alarm info. 
    /// </summary>
    [Serializable]
    public class AlarmForm
    {
        /// <summary>
        /// Name of the alarm
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Time for the alarm
        /// </summary>
        [Required]
        public DateTime? Time { get; set; }

        /// <summary>
        /// Helper method to create the Form
        /// </summary>
        public static IForm<AlarmForm> BuildForm()
        {
            OnCompletionAsyncDelegate<AlarmForm> processAccountQuery = async (context, state) =>
            {
                await context.PostAsync($"Creating alarm named {state.Name} for {state.Time}");
            };

            var builder = new FormBuilder<AlarmForm>()
                .Field(nameof(Name), "What would you like to call your alarm?")
                .Field(nameof(Time), "What time would you like to set the alarm for?")
                .OnCompletion(processAccountQuery);
            var form = builder.Build();
            return form;
        }
    }
}