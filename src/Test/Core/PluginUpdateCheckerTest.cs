using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Plugins;

namespace Test.Core
{
    [TestClass]
    public class PluginUpdateCheckerTest
    {
        [TestMethod]
        public async Task CheckAsyncTest()
        {
            var pluginPath = Environment.GetEnvironmentVariable("plugin_directory", EnvironmentVariableTarget.User);
            const string githubUrl = "https://raw.githubusercontent.com/SubtitleEdit/plugins/master/Plugins4.xml";
            var sut = new PluginUpdateChecker(new PluginUpdateCheckerOptions()
            {
                GithubUrl = githubUrl, PluginDirectory = pluginPath
            });
            var updateCheckResult = await sut.CheckAsync();

            Assert.AreEqual(false, updateCheckResult.Available);
        }
    }
}