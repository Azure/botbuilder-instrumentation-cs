using System;
using System.Collections.Generic;
using BotBuilder.Instrumentation.Instumentation;
using BotBuilder.Instrumentation.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BotBuilder.Instrumentation.Tests
{
    [TestClass]
    public class BotFrameworkApplicationInsightsInstrumentationTests
    {
        [TestMethod]
        public void SentimentManager_ConstructorWithNullArguments_DoesNotThrowException()
        {
            new SentimentManager(null, null, null);
        }

        [TestMethod]
        public void SentimentManager_ConstructorWithEmptyArguments_DoesNotThrowException()
        {
            new SentimentManager("", "", "");
        }

        [TestMethod]
        public void BotFrameworkApplicationInsightsInstrumentation_ConstructorWithNullSettings_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new BotFrameworkApplicationInsightsInstrumentation(null));
        }

        [TestMethod]
        public void BotFrameworkApplicationInsightsInstrumentation_SettingsWithoutInstrumentationKey_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new BotFrameworkApplicationInsightsInstrumentation(new InstrumentationSettings()));
        }

        [TestMethod]
        public void BotFrameworkApplicationInsightsInstrumentation_SettingsWithEmptyInstrumentationKeyList_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>
            (
                () => new BotFrameworkApplicationInsightsInstrumentation(new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>()
                })
            );
        }

        [TestMethod]
        public void BotFrameworkApplicationInsightsInstrumentation_Constructor_DoesNotThrowException()
        {
            new BotFrameworkApplicationInsightsInstrumentation
            (
                new InstrumentationSettings
                {
                    InstrumentationKeys = new List<string>(new[] {"instrumentation key"}),
                    OmitUsernameFromTelemetry = false,
                    SentimentManager = new SentimentManager("", "", "")
                }
            );
        }
    }
}