using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.Cea708.Commands;
using System;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class Cea708Test
    {
        [TestMethod]
        public void CommandClearWindows()
        {
            var command = new ClearWindows(0, new[] { (byte)0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], ClearWindows.Id);
            Assert.AreEqual(bytes[1], 0xff);
        }

        [TestMethod]
        public void CommandDefineWindow()
        {
            var command = new DefineWindow(0, new byte[] { 140, 63, 153, 0, 65, 11, 1 }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 7);
            Assert.AreEqual(bytes[0], (byte)command.Id);
            Assert.AreEqual(bytes[1], 63);
            Assert.AreEqual(bytes[2], 153);
            Assert.AreEqual(bytes[3], 0);
            Assert.AreEqual(bytes[4], 65);
            Assert.AreEqual(bytes[5], 11);
            Assert.AreEqual(bytes[6], 1);
        }

        [TestMethod]
        public void CommandDelay()
        {
            var command = new Delay(0, new[] { (byte)0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], Delay.Id);
            Assert.AreEqual(bytes[1], 0xff);
            Assert.AreEqual(25500, command.Milliseconds);
        }

        [TestMethod]
        public void CommandDelayCancel()
        {
            var command = new DelayCancel(0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 1);
            Assert.AreEqual(bytes[0], DelayCancel.Id);
        }

        [TestMethod]
        public void CommandDeleteWindows()
        {
            var command = new DeleteWindows(0, new[] { (byte)0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], DeleteWindows.Id);
            Assert.AreEqual(bytes[1], 0xff);
        }

        [TestMethod]
        public void CommandDisplayWindows()
        {
            var command = new DisplayWindows(0, new[] { (byte)0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], DisplayWindows.Id);
            Assert.AreEqual(bytes[1], 0xff);
        }

        [TestMethod]
        public void CommandEndOfText()
        {
            var command = new EndOfText(0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 1);
            Assert.AreEqual(bytes[0], EndOfText.Id);
        }

        [TestMethod]
        public void CommandHideWindows()
        {
            var command = new HideWindows(0, new[] { (byte)0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], HideWindows.Id);
            Assert.AreEqual(bytes[1], 0xff);
        }

        [TestMethod]
        public void CommandReset()
        {
            var command = new Reset(0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 1);
            Assert.AreEqual(bytes[0], Reset.Id);
        }

        [TestMethod]
        public void CommandSetCurrentWindow()
        {
            var command = new SetCurrentWindow(0, 1);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 1);
            Assert.AreEqual(bytes[0], SetCurrentWindow.IdStart + 1);
            Assert.AreEqual(command.WindowIndex, 1);
        }

        [TestMethod]
        public void CommandSetPenAttributes()
        {
            var command = new SetPenAttributes(0, new byte[] { 140, 0xff }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 3);
            Assert.AreEqual(bytes[0], SetPenAttributes.Id);
            Assert.AreEqual(bytes[1], 140);
            Assert.AreEqual(bytes[2], 0xff);
        }

        [TestMethod]
        public void CommandSetPenColor()
        {
            var command = new SetPenColor(0, new byte[] { 145, 42, 0 }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 4);
            Assert.AreEqual(bytes[0], SetPenColor.Id);
            Assert.AreEqual(bytes[1], 145);
            Assert.AreEqual(bytes[2], 42);
            Assert.AreEqual(bytes[3], 0);
        }

        [TestMethod]
        public void CommandSetPenLocation()
        {
            var command = new SetPenLocation(0, new byte[] { 2, 4 }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 3);
            Assert.AreEqual(bytes[0], SetPenLocation.Id);
            Assert.AreEqual(bytes[1], 2);
            Assert.AreEqual(bytes[2], 4);
        }

        [TestMethod]
        public void CommandSetWindowAttributes()
        {
            var command = new SetWindowAttributes(0, new byte[] { 140, 255, 153, 0 }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 5);
            Assert.AreEqual(bytes[0], SetWindowAttributes.Id);
            Assert.AreEqual(bytes[1], 140);
            Assert.AreEqual(bytes[2], 255);
            Assert.AreEqual(bytes[3], 153);
            Assert.AreEqual(bytes[4], 0);
        }

        [TestMethod]
        public void CommandTextCommand()
        {
            var command = new SetText(0, "Hallo!");
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 6);
            Assert.AreEqual(bytes[0], 72);
            Assert.AreEqual(bytes[1], 97);
            Assert.AreEqual(bytes[2], 108);
            Assert.AreEqual(bytes[3], 108);
            Assert.AreEqual(bytes[4], 111);
            Assert.AreEqual(bytes[5], 33);
        }

        [TestMethod]
        public void CommandToggleWindows()
        {
            var command = new ToggleWindows(0, new byte[] { 145 }, 0);
            var bytes = command.GetBytes();
            Assert.AreEqual(bytes.Length, 2);
            Assert.AreEqual(bytes[0], ToggleWindows.Id);
            Assert.AreEqual(bytes[1], 145);
        }

        [TestMethod]
        public void CcDataSectionTest()
        {
            var input = new byte[] { 0x72, 0xF4, 0xFC, 0x94, 0x2F, 0xFD, 0x80, 0x80, 0xFF, 0x0C, 0x34, 0xFE, 0x8C, 0xFF, 0xFE, 0x98, 0x00, 0xFE, 0x3C, 0x41, 0xFE, 0x02, 0x29, 0xFE, 0x11, 0x97, 0xFE, 0xD5, 0x15, 0xFE, 0x0C, 0x20, 0xFE, 0x92, 0x00, 0xFE, 0x02, 0x90, 0xFE, 0x05, 0x00, 0xFE, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00 };
            var ccDataSection = new CcDataSection(input, 0);
            var bytes = ccDataSection.GetBytes();
            Assert.AreEqual(20, ccDataSection.CcData.Length);
            Assert.AreEqual(bytes.Length, input.Length);
            for (var index = 0; index < input.Length; index++)
            {
                Assert.AreEqual(input[index], bytes[index]);
            }
        }

        [TestMethod]
        public void CcServiceInfoSectionTest()
        {
            var input = new byte[] { 0x73, 0xF2, 0xE0, 0x20, 0x20, 0x20, 0x7E, 0x7F, 0xFF, 0xE1, 0x65, 0x6E, 0x67, 0xC1, 0x7F, 0xFF };
            var serviceInfoSection = new CcServiceInfoSection(input, 0);
            var bytes = serviceInfoSection.GetBytes();
            Assert.AreEqual(2, serviceInfoSection.CcServiceInfoSectionElements.Length);
            Assert.AreEqual(bytes.Length, input.Length);
            for (var index = 0; index < input.Length; index++)
            {
                Assert.AreEqual(input[index], bytes[index]);
            }
        }

        [TestMethod]
        public void Smpte291MTest()
        {
            var input = new byte[] { 0x61, 0x01, 0x59, 0x96, 0x69, 0x59, 0x4F, 0x7F, 0x00, 0x00, 0x72, 0xF4, 0xFC, 0x94, 0x2F, 0xFD, 0x80, 0x80, 0xFF, 0x03, 0x22, 0xFE, 0x8A, 0xFF, 0xFE, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0xFA, 0x00, 0x00, 0x73, 0xF2, 0xE0, 0x20, 0x20, 0x20, 0x7E, 0x7F, 0xFF, 0xE1, 0x65, 0x6E, 0x67, 0xC1, 0x7F, 0xFF, 0x74, 0x00, 0x00, 0xFA, 0xBB };
            var smpte291M = new Smpte291M(input);
            var bytes = smpte291M.GetBytes();
            Assert.AreEqual(bytes.Length, input.Length);
            for (var index = 0; index < input.Length; index++)
            {
                Assert.AreEqual(input[index], bytes[index]);
            }
        }

        [TestMethod]
        public void VancTest()
        {
            var s = VancDataWriter.GenerateLinesFromText("Hi!", 0)[0];
            var smpte291M = new Smpte291M(HexStringToByteArray(s));
            var result = smpte291M.GetText(0, true, new CommandState());
            Assert.AreEqual("Hi!", result);
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars - 1; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}
