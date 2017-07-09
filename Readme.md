# BotBuilder Instrumantation (C#)
This module is used to add instrumentation to bots built with [Microsoft Bot Framework](https://dev.botframework.com/). 
You can leverage the events from this module using [Ibex Dashboard](https://github.com/CatalystCode/ibex-dashboard).

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
Add the following keys to appSettings section.
```xml
    <!-- AppInsights InstrumentationKey-->
    <add key="InstrumentationKey" value="17b45976-7f04-4f49-a771-3446788959e0" />
    <add key="InstrumentationShouldOmitUsernameFromTelemetry" value="0"/>
    
	<!-- All following keys are optional:-->
    <!-- LUIS credentials-->
    <add key="LuisModelId" value="0a2cc164-5a19-47b7-b85e-41914d9037ba" />
    <add key="LuisSubscriptionKey" value="d7b46a6c72bf46c1b67f2c4f21acf960" />
    
    <!-- Text Analytics data for message sentiment analysis -->
    <add key="TextAnalyticsApiKey" value="d19acc35642b4ce4876199b8b39d6ba3" />
    <add key="TextAnalyticsMinLength" value="3" />
    <add key="CognitiveServiceApiEndpoint" value="https://westus.api.cognitive.microsoft.com/"/>
```
* `InstrumentationKey`. Is your Application Insights instrumentation key, you can obtain this key from Azure Portal once you configure your web application to use application insights.
* `TextAnalyticsApiKey`. To track sentiment analysis, the telemetry code calls the Text Analytics API, you can obtain this key from the Azure Portal. The bot won't log any sentiment data if this value is empty.
* `TextAnalyticsMinLength`. You normally don't want to track sentiment for short phrases like "yes", "no", etc. In the example above, this parameter tells the logger to only track sentiment for messages that have 3 words or more. 

### Initialize the bot builder instrumentation
Since the code is thread safe, and Application Insights SDK recommends to create a single instance per application of the SDK,
we also recommed creating a single instance of the bot builder instrumentation.
You can achieve that in various ways such as creating a readonly singleton,
or for example using an IoC container to keep a single instance with per-lifetime restriction.
```cs
	//Singleton implementation 
    public static readonly BotFrameworkApplicationInsightsInstrumentation DefaultInstrumentation = new BotFrameworkApplicationInsightsInstrumentation(
        new BotBuilder.Instrumentation.Instumentation.InstrumentationSettings
        {
            InstrumentationKeys = new List<string>(new string[] { ConfigurationManager.AppSettings["InstrumentationKey"] }),
            OmitUsernameFromTelemetry = Convert.ToBoolean(ConfigurationManager.AppSettings["InstrumentationShouldOmitUsernameFromTelemetry"]),
            SentimentManager = new SentimentManager(
                        ConfigurationManager.AppSettings["TextAnalyticsApiKey"],
                        ConfigurationManager.AppSettings["TextAnalyticsMinLength"],
                        ConfigurationManager.AppSettings["CognitiveServiceApiEndpoint"]
                        )
        });
```
The call above, will automatically start monitoring your Bots Dialogs and send telemtry to Application Insights.

### Tracking LUIS intents
To tack and LUIS intents all you need to do is inherit your Dialog from InstrumentedLuisDialog
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
Hook into the result function of QNA to extract relevant data
```
DefaultInstrumentation.TrackQnaEvent(activity, userQuery, kbQuestion, kbAnswer, score);
```
You can see how to implement a QnA service [here](https://github.com/Microsoft/BotBuilder-CognitiveServices/tree/master/CSharp/Samples/QnAMaker/QnABotWithOverrides)


## Tracking custom events
You can send your own custom properties as `Dictionary<string, string>` to the telemetry.
```
DefaultInstrumentation.TrackCustomEvent(activity, customEventProperties);
```


## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

