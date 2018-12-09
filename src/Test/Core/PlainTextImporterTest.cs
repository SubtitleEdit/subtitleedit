using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Core
{
    [TestClass]
    public class PlainTextImporterTest
    {
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce feugiat, est id ultricies auctor, lorem dolor auctor sapien, vel cursus justo sapien vel metus. In risus nibh, sodales vel consectetur fermentum, molestie et risus. Nulla placerat feugiat tellus id sollicitudin. Quisque aliquet tincidunt nisi non tristique. Nullam egestas nulla ac quam ultrices, in pretium odio tempus. Nulla in pretium tellus, eu tincidunt sem. In vel bibendum nisi, imperdiet tempus neque. Vestibulum sit amet est sed orci faucibus venenatis. Nulla aliquam venenatis erat, vel elementum mi semper in. Quisque iaculis aliquam euismod. Vivamus tellus diam, congue non luctus eget, semper et nisi. Nunc id lacus vitae nibh auctor tempus sed sit amet ipsum.";

        [TestMethod]
        public void PlainTextImportAutoSplit1()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: false, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var lines = new List<string>
            {
                "How are you? I'm fine.",
                "",
                "Last line."
            };
            var result = importer.ImportAutoSplit(lines);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("How are you?", result[0]);
            Assert.AreEqual("I'm fine.", result[1]);
            Assert.AreEqual("Last line.", result[2]);
        }

        [TestMethod]
        public void PlainTextImportAutoSplit2()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: false, numberOfLines: 2, endChars: "", singleLineMaxLength: 43, language: "en");
            var lines = new List<string>
            {
                "How are you? I'm fine.",
                "",
                "Last line."
            };
            var result = importer.ImportAutoSplit(lines);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("How are you? I'm fine.", result[0]);
            Assert.AreEqual("Last line.", result[1]);
        }


        [TestMethod]
        public void PlainTextImportAutoSplit3()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: false, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var lines = new List<string>
            {
                "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789",
            };
            var result = importer.ImportAutoSplit(lines);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void PlainTextImportAutoSplit4()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: false, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var lines = new List<string>
            {
                "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789",
            };
            var result = importer.ImportAutoSplit(lines);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void PlainTextImportAutoSplit5()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: true, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var lines = new List<string>
            {
                "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789",
            };
            var result = importer.ImportAutoSplit(lines);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PlainTextImportAutoSplit_Only_one_Line()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: true, numberOfLines: 1, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var result = importer.ImportAutoSplit(new List<string> { LoremIpsum });
            Assert.IsFalse(result.Any(p => p.Contains(Environment.NewLine)));
        }

        [TestMethod]
        public void PlainTextImportAutoSplit_Two_Lines()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: true, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var result = importer.ImportAutoSplit(new List<string> { LoremIpsum });
            Assert.IsTrue(result.Any(p => p.Contains(Environment.NewLine)));
        }

        [TestMethod]
        public void PlainTextImportAutoSplit_Max_Single_Line_Length()
        {
            var importer = new PlainTextImporter(splitAtBlankLines: true, removeLinesWithoutLetters: true, numberOfLines: 2, endChars: ".!?", singleLineMaxLength: 43, language: "en");
            var result = importer.ImportAutoSplit(new List<string> { LoremIpsum });
            foreach (var line in result)
            {
                foreach (var s in line.SplitToLines())
                {
                    Assert.IsFalse(s.Length > 43);
                }
            }
        }
    }
}
