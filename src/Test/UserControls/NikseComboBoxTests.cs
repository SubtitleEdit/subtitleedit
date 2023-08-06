using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Controls;
using System.Windows.Forms;

namespace Test.UserControls
{
    [TestClass]
    public class NikseComboBoxTests
    {
        [TestMethod]
        public void NikseComboBox()
        {
            
            var cbNormal = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList};
            var normalSelectedIndexChangedCount = 0;
            cbNormal.SelectedIndexChanged += (sender, args) => { normalSelectedIndexChangedCount++; };
            var normalSelectedValueChangedCount = 0;
            cbNormal.SelectedValueChanged += (sender, args) => { normalSelectedValueChangedCount++; };

            var cbNikse = new NikseComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            var nikseSelectedIndexChangedCount = 0;
            cbNikse.SelectedIndexChanged += (sender, args) => { nikseSelectedIndexChangedCount++; };
            var nikseSelectedValueChangedCount = 0;
            cbNikse.SelectedValueChanged += (sender, args) => { nikseSelectedValueChangedCount++; };

            cbNormal.Items.Add("Test");
            cbNikse.Items.Add("Test");

            Verify(normalSelectedIndexChangedCount, nikseSelectedIndexChangedCount, normalSelectedValueChangedCount, nikseSelectedValueChangedCount, cbNormal, cbNikse);

            cbNormal.SelectedIndex = 0;
            cbNikse.SelectedIndex = 0;

            Verify(normalSelectedIndexChangedCount, nikseSelectedIndexChangedCount, normalSelectedValueChangedCount, nikseSelectedValueChangedCount, cbNormal, cbNikse);

            cbNormal.Items.Clear();
            cbNikse.Items.Clear();

            Verify(normalSelectedIndexChangedCount, nikseSelectedIndexChangedCount, normalSelectedValueChangedCount, nikseSelectedValueChangedCount, cbNormal, cbNikse);
        }

        private static void Verify(int normalSelectedIndexChangedCount, int nikseSelectedIndexChangedCount, int normalSelectedValueChangedCount, int nikseSelectedValueChangedCount, ComboBox cbNormal, NikseComboBox cbNikse)
        {
            Assert.AreEqual(normalSelectedIndexChangedCount, nikseSelectedIndexChangedCount);
            Assert.AreEqual(normalSelectedValueChangedCount, nikseSelectedValueChangedCount);
            Assert.AreEqual(cbNormal.SelectedIndex, cbNikse.SelectedIndex);
            //Assert.AreEqual(cbNormal.Text, cbNikse.Text);
            Assert.AreEqual(cbNormal.SelectedItem, cbNikse.SelectedItem);
            //Assert.AreEqual(cbNormal.SelectedText, cbNikse.SelectedText);
        }
    }
}
