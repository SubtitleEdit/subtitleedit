using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Cea708.Commands;

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
            var command = new TextCommand(0, "Hallo!");
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
    }
}
