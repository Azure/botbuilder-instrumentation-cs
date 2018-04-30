# BotBuilder Instrumentation (C#)

[![NuGet](https://img.shields.io/nuget/v/BotBuilder.Instrumentation.svg)](https://www.nuget.org/packages/BotBuilder.Instrumentation) [![Build status](https://ci.appveyor.com/api/projects/status/dguorsl5dwrygt2k/branch/master?svg=true)](https://ci.appveyor.com/project/syedhassaanahmed/botbuilder-instrumentation-cs/branch/master)

This module is used to add instrumentation to bots built with [Microsoft Bot Framework](https://dev.botframework.com/). 
You can leverage the events from this module using [Ibex Dashboard](https://github.com/CatalystCode/ibex-dashboard).

### Via Nuget
Visit us on [nuget.org](https://www.nuget.org/packages/BotBuilder.Instrumentation/)
Or add it to your Visual Studio project by running the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console): 
`PM> Install-Package BotBuilder.Instrumentation`

### Sample bot with instrumentation included
To see this DLL in action, you can take a look at this [https://github.com/itye-msft/Bot-with-instrumentation-cs](https://github.com/itye-msft/Bot-with-instrumentation-cs) C# project, which is a working bot sample using instrumentation library.

## Getting Started

1. Create an Application Insights service under your subscription. (for more information on App Insights see [Set up Application Insights for ASP.NET](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-asp-net))
2. Use the `Instrumentation Key` inside your bot registration page under _Instrumentation key_.
3. Under the App Insights service, go to **API Access** and copy **Application ID**
4. Under the App Insights service, go to **API Access >> New Key** with _Read_ permissions and copy **Api Key**.

## Connect to Cognitive Services
This is an optional step in case you want user messages to be analised for setiments.
Create a new [Sentiment Analisys Service under Cognitive Services](https://www.microsoft.com/cognitive-services/en-us/text-analytics-api).
When creating the service, make sure to mark **Text Analytics - Preview**.

### Setting up web.config.
If you are not using cognitive services/LUIS and you just want to automatically send conversation data to the telemetry, all you need to add is your instrumentation key, and a flag to send(or not) username.
```xml
    <!-- AppInsights InstrumentationKey-->
    <add key="InstrumentationKey" value="17b45976-7f04-4f49-a771-3446788959e0" />
    <add key="InstrumentationShouldOmitUsernameFromTelemetry" value="False"/>
```
* `InstrumentationKey`. Is your Application Insights instrumentation key, you can obtain this key from Azure Portal once you configure your web application to use application insights.

In case you are using cognitive service and LUIS, you also need to add the following keys:
```xml
    <!-- LUIS credentials-->
    <add key="LuisModelId" value="0a2cc164-5a19-47b7-b85e-41914d9037ba" />
    <add key="LuisSubscriptionKey" value="d7b46a6c72bf46c1b67f2c4f21acf960" />
    
    <!-- Text Analytics data for message sentiment analysis -->
    <add key="TextAnalyticsApiKey" value="d19acc35642b4ce4876199b8b39d6ba3" />
    <add key="TextAnalyticsMinLength" value="3" />
    <add key="CognitiveServiceApiEndpoint" value="https://westus.api.cognitive.microsoft.com/"/>
```
* `TextAnalyticsApiKey`. To track sentiment analysis, the telemetry code calls the Text Analytics API, you can obtain this key from the Azure Portal. The bot won't log any sentiment data if this value is empty.
* `TextAnalyticsMinLength`. You normally don't want to track sentiment for short phrases like "yes", "no", etc. In the example above, this parameter tells the logger to only track sentiment for messages that have 3 words or more. 

### Initialize the bot builder instrumentation
Since the code is thread safe, and Application Insights SDK recommends to create a single instance per application of the SDK,
we also recommed creating a single instance of the bot builder instrumentation.
You can achieve that in various ways such as creating a readonly singleton,
or for example using an IoC container to keep a single instance with per-lifetime restriction.

Basic sample with no cognitive services:
```cs
    public readonly BotFrameworkApplicationInsightsInstrumentation DefaultInstrumentation = DependencyResolver.Current.DefaultBasicInstrumentation;
```
If you are using cognitive services, than you can initialize the singleton as follows:
```cs
    public readonly BotFrameworkApplicationInsightsInstrumentation DefaultInstrumentation = DependencyResolver.Current.DefaultInstrumentationWithCognitiveServices;
```
The call above, will automatically start monitoring your Bots Dialogs and send telemtry to Application Insights.

### Tracking LUIS intents
To track any LUIS intents all you need to do is inherit your Dialog from InstrumentedLuisDialog
```cs
[Serializable]
public class RootDialog : InstrumentedLuisDialog<object>
{
    public RootDialog(string luisModelId, string luisSubscriptionKey) : base(luisModelId, luisSubscriptionKey)
    {
    }
	// ...
}
```
This call will log `LuisResults` to application insights, including their score and any entities identified by LUIS. 
Although it's optional, it is recommended if you're using [Ibex Dashboard](https://github.com/CatalystCode/ibex-dashboard), in which case, adding sentiment analytsis will add sentiments overview to the dashboard along with a sentiment icon next to all conversations.

### Tracking other messages
There are situations where you may need to send a message to the user outside the context of a dialog posting directly to the conversation. 
In these cases, you  need to manually invoke the `await DefaultInstrumentation.TrackActivity(message);` method.  

## Tracking QnA Maker events
[Ibex Dashboard](https://github.com/CatalystCode/ibex-dashboard) has a built-in QnA dashboard which you can easily use.
Hook into the result function of QNA to extract relevant data:
```
DefaultInstrumentation.TrackQnaEvent(activity, userQuery, kbQuestion, kbAnswer, score);
```
You can see how to implement a QnA service [here](https://github.com/Microsoft/BotBuilder-CognitiveServices/tree/master/CSharp/Samples/QnAMaker/QnABotWithOverrides) and in particular [QnADialogWithOverrides](https://github.com/Microsoft/BotBuilder-CognitiveServices/blob/master/CSharp/Samples/QnAMaker/QnABotWithOverrides/Dialogs/QnADialogWithOverrides.cs)

## Tracking generic Goals
You can trigger generic goals, much like the way a Goal can be triggered on a web site in Google Analytics
```
DefaultInstrumentation.TrackGoalTriggeredEvent(activity, goalName, goalTriggeredEventProperties);
```

## Tracking custom events
You can send your own custom properties as `Dictionary<string, string>` to the telemetry.
```
DefaultInstrumentation.TrackCustomEvent(activity, customEventProperties);
```
## Performance benchmarks
The SDK is performance tested using [BenchmarkDotNet](http://benchmarkdotnet.org/Overview.htm) library. The gist of this implementation is in project [BotBuilder.Instrumentation.Benchmarks](https://github.com/CatalystCode/botbuilder-instrumentation-cs/tree/master/BotBuilder.Instrumentation.Benchmarks) where all publicly exposed methods of the SDK are benchmarked. Typical results on our local dev environment looks like following (where `us` = Microseconds);

``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-6820HQ CPU 2.70GHz (Skylake), ProcessorCount=8
Frequency=2648437 Hz, Resolution=377.5812 ns, Timer=TSC
  [Host]   : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2098.0DEBUG [AttachedDebugger]
  ShortRun : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2098.0

Job=ShortRun  LaunchCount=1  TargetCount=3  
WarmupCount=3  

```
 |           Method |      Mean |     Error |   StdDev |       Min |       Max |
 |----------------- |----------:|----------:|---------:|----------:|----------:|
 |    TrackActivity | 180.58 us | 135.70 us | 7.667 us | 172.93 us | 188.27 us |
 |  TrackLuisIntent |  78.54 us |  43.36 us | 2.450 us |  76.05 us |  80.94 us |
 |    TrackQnaEvent |  80.65 us |  37.23 us | 2.104 us |  78.23 us |  81.98 us |
 | TrackCustomEvent |  80.35 us | 100.14 us | 5.658 us |  75.34 us |  86.48 us |
