//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Nikse.SubtitleEdit.Logic.Plugins;
//using NSubstitute;
//using NSubstitute.Extensions;

//namespace Test.Core
//{
//    [TestClass]
//    public class PluginUpdateCheckerTest
//    {
//        [TestMethod]
//        public async Task CheckUpdateTest()
//        {
//            // Arrange
//            var onlinePluginProvider = Substitute.For<IOnlinePluginMetadataProvider>();
//            var localPluginProvider = Substitute.For<ILocalPluginMetadataProvider>();

//            // configure
//            onlinePluginProvider.Configure()
//                .GetPluginsAsync()
//                .Returns(new List<PluginInfoItem>()
//                {
//                    new LocalPlugin("foobar", "foobar", 2.0m)
//                });
//            localPluginProvider.Configure()
//                .GetInstalledPlugins()
//                .Returns(new List<LocalPlugin>()
//                {
//                    new LocalPlugin("foobar", "foobar", 1.0m)
//                });

//            var sut = new PluginUpdateChecker(localPluginProvider, onlinePluginProvider);

//            // Act 
//            var updateCheckResult = await sut.CheckAsync().ConfigureAwait(false);

//            // Assert 
//            Assert.AreEqual(true, updateCheckResult.Available);
//        }
        
//        [TestMethod]
//        public async Task NoPluginInstalledTest()
//        {
//            // Arrange
//            var onlinePluginProvider = Substitute.For<IOnlinePluginMetadataProvider>();
//            var localPluginProvider = Substitute.For<ILocalPluginMetadataProvider>();
//            var sut = new PluginUpdateChecker(localPluginProvider, onlinePluginProvider);
            
//            // Act 
//            var updateCheckResult = await sut.CheckAsync();

//            // Assert 
//            Assert.AreEqual(false, updateCheckResult.Available);
//            Assert.AreEqual(false, updateCheckResult.PluginUpdates.Any());
//        }

//        [TestMethod]
//        public async Task NoOnlinePluginAvailableTest()
//        {
//            // Arrange
//            var onlinePluginProvider = Substitute.For<IOnlinePluginMetadataProvider>();
//            var localPluginProvider = Substitute.For<ILocalPluginMetadataProvider>();
//            localPluginProvider.Configure().GetInstalledPlugins().Returns(new List<LocalPlugin>()
//            {
//                new LocalPlugin("foobar", "foobar", 1m)
//            });

//            var sut = new PluginUpdateChecker(localPluginProvider, onlinePluginProvider);
//            var updateCheckResult = await sut.CheckAsync();

//            // Act 

//            // Assert 
//            Assert.AreEqual(false, updateCheckResult.Available);
//            Assert.AreEqual(false, updateCheckResult.PluginUpdates.Any());
//        }
//    }
//}