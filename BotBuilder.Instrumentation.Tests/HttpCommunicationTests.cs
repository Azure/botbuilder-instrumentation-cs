using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BotBuilder.Instrumentation.Tests
{
    [TestClass]
    public class HttpCommunicationTests
    {
        [TestMethod]
        public async Task PostAsync_NullBaseEndpoint_ThrowsArgumentException()
        {
            var httpCommunication = new HttpCommunication();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await httpCommunication.PostAsync(null, "", new Dictionary<string, string>(), new byte[0]));
        }

        [TestMethod]
        public async Task PostAsync_InvalidBaseEndpoint_ThrowsArgumentException()
        {
            var httpCommunication = new HttpCommunication();
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () =>
                    await httpCommunication.PostAsync("invalid uri", "", new Dictionary<string, string>(), new byte[0]));
        }

        [TestMethod]
        public async Task PostAsync_NullRoute_ThrowsArgumentNullException()
        {
            var httpCommunication = new HttpCommunication();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () =>
                    await httpCommunication.PostAsync("http://localhost", null, new Dictionary<string, string>(),
                        new byte[0]));
        }

        [TestMethod]
        public async Task PostAsync_NullData_ThrowsArgumentNullException()
        {
            var httpCommunication = new HttpCommunication();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                async () =>
                    await httpCommunication.PostAsync("http://localhost", "", new Dictionary<string, string>(), null));
        }
    }
}
