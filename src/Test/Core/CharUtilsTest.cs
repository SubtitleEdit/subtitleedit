using Nikse.SubtitleEdit.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Core
{
    [TestClass]
    public class CharUtilsTest
    {
        [TestMethod]
        public void IsDigit()
        {
            Assert.AreEqual(true, CharUtils.IsDigit('0'));
            Assert.AreEqual(true, CharUtils.IsDigit('1'));
            Assert.AreEqual(true, CharUtils.IsDigit('2'));
            Assert.AreEqual(true, CharUtils.IsDigit('3'));
            Assert.AreEqual(true, CharUtils.IsDigit('4'));
            Assert.AreEqual(true, CharUtils.IsDigit('5'));
            Assert.AreEqual(true, CharUtils.IsDigit('6'));
            Assert.AreEqual(true, CharUtils.IsDigit('7'));
            Assert.AreEqual(true, CharUtils.IsDigit('8'));
            Assert.AreEqual(true, CharUtils.IsDigit('9'));

            Assert.AreEqual(false, CharUtils.IsDigit('.'));
            Assert.AreEqual(false, CharUtils.IsDigit('A'));
            Assert.AreEqual(false, CharUtils.IsDigit(' '));
            Assert.AreEqual(false, CharUtils.IsDigit('z'));
        }

        [TestMethod]
        public void IsEnglishAlphabet()
        {
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('a'));
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('b'));
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('z'));
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('A'));
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('Y'));
            Assert.AreEqual(true, CharUtils.IsEnglishAlphabet('Z'));

            Assert.AreEqual(false, CharUtils.IsEnglishAlphabet('æ'));
            Assert.AreEqual(false, CharUtils.IsEnglishAlphabet('ü'));
            Assert.AreEqual(false, CharUtils.IsEnglishAlphabet('2'));
            Assert.AreEqual(false, CharUtils.IsEnglishAlphabet('!'));
        }
    }
}
