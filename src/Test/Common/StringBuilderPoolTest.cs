using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Common
{
    [TestClass]
    public class StringBuilderPoolTest
    {
        [TestMethod]
        public void GetNotNullTest()
        {
            var sb = StringBuilderPool.Get();
            Assert.IsNotNull(sb);
        }

        [TestMethod]
        public void GetAssertMinCapacityTest()
        {
            // assert init with the min capacity
            Assert.AreEqual(1024, StringBuilderPool.Get().Capacity);
        }
        
        [TestMethod]
        public void ReturnToPoolTest()
        {
            var sbOne = StringBuilderPool.Get();
            var sbTwo = StringBuilderPool.Get();
            var sbThree = StringBuilderPool.Get();

            // return all to pool
            var isOneReturned = sbOne.ReturnToPool();
            var isTwoReturned = sbTwo.ReturnToPool();
            var isThreeReturned = sbThree.ReturnToPool();
            
            Assert.IsTrue(isOneReturned);
            Assert.IsTrue(isTwoReturned);
            Assert.IsTrue(isThreeReturned);
        }
        
        [TestMethod]
        public void ToPoolTest()
        {
            const string content = "the quick brown fox jumps over a lazy dog.";
            var sb = StringBuilderPool.Get();
            sb.Append(content);
            Assert.AreEqual(content, sb.ToPool());
            
            var freshSbFromPool = StringBuilderPool.Get();
            // we are expecting the same string builder reference
            Assert.AreEqual(freshSbFromPool, sb);
            Assert.AreEqual(0, freshSbFromPool.Length);
        }

        [TestMethod]
        public void ReturnToPoolCapacityGreaterThanMax()
        {
            // arrange
            var sb = StringBuilderPool.Get();
            
            // act
            sb.Capacity = 85001;
            
            // assert
            Assert.IsFalse(sb.ReturnToPool());
        }
        
        [TestMethod]
        public void ReturnToPoolAlreadyFill()
        {
            // arrange
            var sbOne = StringBuilderPool.Get();
            var sbTwo = StringBuilderPool.Get();
            var sbThree = StringBuilderPool.Get();
            var sbFour = StringBuilderPool.Get();
            
            // act &&  assert
            Assert.IsTrue(sbOne.ReturnToPool());
            Assert.IsTrue(sbTwo.ReturnToPool());
            Assert.IsTrue(sbThree.ReturnToPool());
            Assert.IsFalse(sbFour.ReturnToPool());
        }
    }
}