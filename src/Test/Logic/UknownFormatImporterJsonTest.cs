using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Logic
{
    [TestClass]
    public class UknownFormatImporterJsonTest
    {
        [TestMethod]
        public void TestUnknownJson1()
        {
            var raw = @"{
        'subtitles': [
        {
            'sub_order' : 1,
            'text' : 'this presentation is delivered by the stanford center for professional',
            'start_time' : 13.128,
            'end_time' : 16.399
        },        {
            'sub_order' : 2,
            'text' : 'development',
            'start_time' : 16.399,
            'end_time' : 23.399
        },        {
            'sub_order' : 3,
            'text' : 'welcome welcome to seattle on six people in the next actions',
            'start_time' : 27.07,
            'end_time' : 30.099
        },        {
            'sub_order' : 4,
            'text' : 'may not make it on',
            'start_time' : 30.099,
            'end_time' : 31.949
        },        {
            'sub_order' : 5,
            'text' : 'armitage technocrat',
            'start_time' : 31.949,
            'end_time' : 33.57
        },        {
            'sub_order' : 6,
            'text' : 'and',
            'start_time' : 33.57,
            'end_time' : 34.63
        },        {
            'sub_order' : 7,
            'text' : 'their website actually was for the most important thing to take away from here',
            'start_time' : 34.63,
            'end_time' : 37.66
        },        {
            'sub_order' : 8,
            'text' : 'right where can you find information of the class going to talk today get some',
            'start_time' : 37.659,
            'end_time' : 40.179
        },        {
            'sub_order' : 9,
            'text' : 'of the inside like that but this is kinda home base fort',
            'start_time' : 40.179,
            'end_time' : 42.959
        },        {
            'sub_order' : 10,
            'text' : 'all the materials have you managed to get the handouts on the way in your',
            'start_time' : 42.959,
            'end_time' : 45.339
        },        {
            'sub_order' : 11,
            'text' : 'golden otherwise you can grab from the website there&#039;s a lot of just back when',
            'start_time' : 45.338,
            'end_time' : 48.368
        }]
}";

            var importer = new UnknownFormatImporterJson();
            var subtitle = importer.AutoGuessImport(raw.Replace('\'', '"').SplitToLines());
            Assert.AreEqual(11, subtitle.Paragraphs.Count);
            Assert.AreEqual("development", subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void TestUnknownJsonArray()
        {
            const string raw = @"{
        'subtitles': [
        {
            'sub_order' : 1,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 13.128,
            'end_time' : 16.399
        },        {
            'sub_order' : 2,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 16.399,
            'end_time' : 23.399
        },        {
            'sub_order' : 3,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 27.07,
            'end_time' : 30.099
        },        {
            'sub_order' : 4,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 30.099,
            'end_time' : 31.949
        },        {
            'sub_order' : 5,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 31.949,
            'end_time' : 33.57
        },        {
            'sub_order' : 6,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 33.57,
            'end_time' : 34.63
        },        {
            'sub_order' : 7,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 34.63,
            'end_time' : 37.66
        },        {
            'sub_order' : 8,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 37.659,
            'end_time' : 40.179
        },        {
            'sub_order' : 9,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 40.179,
            'end_time' : 42.959
        },        {
            'sub_order' : 10,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 42.959,
            'end_time' : 45.339
        },        {
            'sub_order' : 11,
            'text' : [ 'Ford', 'BMW', 'Fiat' ],
            'start_time' : 45.338,
            'end_time' : 48.368
        }]
}";

            var importer = new UnknownFormatImporterJson();
            var subtitle = importer.AutoGuessImport(raw.Replace('\'', '"').SplitToLines());
            Assert.AreEqual(11, subtitle.Paragraphs.Count);
            Assert.AreEqual("Ford" + Environment.NewLine + "BMW" + Environment.NewLine + "Fiat", subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void ImportBilibiliJson()
        {
            const string raw = @"{
            'body': [
            {
                'from': 3.7,
                'to': 7.7,
                'location': 2,
                'content': 'Line0'
            },
            {
                'from': 7.7,
                'to': 13.7,
                'location': 2,
                'content': 'Line1'
            },
            {
                'from': 13.7,
                'to': 18.7,
                'location': 2,
                'content': 'Line2'
            },
            {
                'from': 18.7,
                'to': 24.7,
                'location': 2,
                'content': 'Line3'
            }]
        }";

            var importer = new UnknownFormatImporterJson();
            var subtitle = importer.AutoGuessImport(raw.Replace('\'', '"').SplitToLines());
            Assert.AreEqual(4, subtitle.Paragraphs.Count);
            Assert.AreEqual("Line1", subtitle.Paragraphs[1].Text);
        }

    }
}