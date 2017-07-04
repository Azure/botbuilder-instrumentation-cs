using LightInject;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    
    public class IoC
    {
        public static readonly LightInject.ServiceContainer Container = new LightInject.ServiceContainer();

        // A helper method for this project
        public static BotBuilder.Instrumentation.Interfaces.IBotFrameworkInstrumentation GetBotInstrumentation()
        {
            return Container.GetInstance<BotBuilder.Instrumentation.Interfaces.IBotFrameworkInstrumentation>();
        }
    }
}