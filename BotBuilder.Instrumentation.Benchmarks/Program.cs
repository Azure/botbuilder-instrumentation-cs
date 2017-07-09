using BenchmarkDotNet.Running;

namespace BotBuilder.Instrumentation.Benchmarks
{
    public class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<BotFrameworkApplicationInsightsInstrumentationTests>();
        }
    }
}
