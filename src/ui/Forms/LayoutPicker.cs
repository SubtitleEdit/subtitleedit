using System;
using System.Drawing;
using System.Drawing.Imaging;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class LayoutPicker : Form
    {
        private int _layout;

        private Bitmap _gray1;
        private Bitmap _gray2;
        private Bitmap _gray3;
        private Bitmap _gray4;
        private Bitmap _gray5;
        private Bitmap _gray6;
        private Bitmap _gray7;
        private Bitmap _gray8;
        private Bitmap _gray9;

        private bool _loading = true;

        public bool ShowVideoControls { get; set; }

        public LayoutPicker(int initialLayout, bool showVideoControls)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Main.ChooseLayout;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxHideVideoControls.Text = LanguageSettings.Current.Main.ShowVideoControls;
            checkBoxHideVideoControls.Checked = showVideoControls;
            ShowVideoControls = showVideoControls;
            CancelButton = buttonCancel;
            AcceptButton = buttonOk;
            _layout = initialLayout;
            UpdateButtons(initialLayout);
        }

        private void UpdateButtons(int initialLayout)
        {
            _gray1?.Dispose();
            _gray2?.Dispose();
            _gray3?.Dispose();
            _gray4?.Dispose();
            _gray5?.Dispose();
            _gray6?.Dispose();
            _gray7?.Dispose();
            _gray8?.Dispose();
            _gray9?.Dispose();

            _gray1 = GrayScale(Properties.Resources.L1, initialLayout == 0);
            _gray2 = GrayScale(Properties.Resources.L2, initialLayout == 1);
            _gray3 = GrayScale(Properties.Resources.L3, initialLayout == 2);
            _gray4 = GrayScale(Properties.Resources.L4, initialLayout == 3);
            _gray5 = GrayScale(Properties.Resources.L5, initialLayout == 4);
            _gray6 = GrayScale(Properties.Resources.L6, initialLayout == 5);
            _gray7 = GrayScale(Properties.Resources.L7, initialLayout == 6);
            _gray8 = GrayScale(Properties.Resources.L8, initialLayout == 7);
            _gray9 = GrayScale(Properties.Resources.L9, initialLayout == 8);

            button1.Image = _gray1;
            button2.Image = _gray2;
            button3.Image = _gray3;
            button4.Image = _gray4;
            button5.Image = _gray5;
            button6.Image = _gray6;
            button7.Image = _gray7;
            button8.Image = _gray8;
            button9.Image = _gray9;

            button1.Font = new Font(button1.Font.FontFamily, 28);
            button2.Font = new Font(button1.Font.FontFamily, 28);
            button3.Font = new Font(button1.Font.FontFamily, 28);
            button4.Font = new Font(button1.Font.FontFamily, 28);
            button5.Font = new Font(button1.Font.FontFamily, 28);
            button6.Font = new Font(button1.Font.FontFamily, 28);
            button7.Font = new Font(button1.Font.FontFamily, 28);
            button8.Font = new Font(button1.Font.FontFamily, 28);
            button9.Font = new Font(button1.Font.FontFamily, 28);
        }

        public static Bitmap GrayScale(Bitmap original, bool selected)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                var colorMatrix = new ColorMatrix(new[]
                {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new[] {0f, 0f, 0f, 1f, 0f},
                    new[] {0f, 0f, 0f, 0f, 1f}
                });

                if (selected)
                {
                    colorMatrix = new ColorMatrix(new[]
                    {
                        new[] {.3f, .3f, .7f, 0, 0},
                        new[] {.59f, .59f, .99f, 0, 0},
                        new[] {.11f, .11f, .11f, 0, 0},
                        new[] {0f, 0f, .3f, 1f, 0f},
                        new[] {0f, 0f, .3f, 0f, 1f}
                    });
                }

                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return newBitmap;
        }

        public int GetLayout()
        {
            return _layout;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _layout = 0;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _layout = 1;
            DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _layout = 2;
            DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _layout = 3;
            DialogResult = DialogResult.OK;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _layout = 4;
            DialogResult = DialogResult.OK;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _layout = 5;
            DialogResult = DialogResult.OK;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _layout = 6;
            DialogResult = DialogResult.OK;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _layout = 7;
            DialogResult = DialogResult.OK;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            _layout = 8;
            DialogResult = DialogResult.OK;
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.Image = Properties.Resources.L1;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.Image = _gray1;
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            button2.Image = Properties.Resources.L2;
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.Image = _gray2;
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            button3.Image = Properties.Resources.L3;
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.Image = _gray3;
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            button4.Image = Properties.Resources.L4;
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.Image = _gray4;
        }

        private void button5_MouseEnter(object sender, EventArgs e)
        {
            button5.Image = Properties.Resources.L5;
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            button5.Image = _gray5;
        }

        private void button6_MouseEnter(object sender, EventArgs e)
        {
            button6.Image = Properties.Resources.L6;
        }

        private void button6_MouseLeave(object sender, EventArgs e)
        {
            button6.Image = _gray6;
        }

        private void button7_MouseEnter(object sender, EventArgs e)
        {
            button7.Image = Properties.Resources.L7;
        }

        private void button7_MouseLeave(object sender, EventArgs e)
        {
            button7.Image = _gray7;
        }

        private void button8_MouseEnter(object sender, EventArgs e)
        {
            button8.Image = Properties.Resources.L8;
        }

        private void button8_MouseLeave(object sender, EventArgs e)
        {
            button8.Image = _gray8;
        }

        private void button9_MouseEnter(object sender, EventArgs e)
        {
            button9.Image = Properties.Resources.L9;
        }

        private void button9_MouseLeave(object sender, EventArgs e)
        {
            button9.Image = _gray9;
        }

        private void LayoutPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down)
            {
                if (_layout > 0)
                {
                    _layout--;
                    UpdateButtons(_layout);
                    Refresh();
                }
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up)
            {
                if (_layout < 8)
                {
                    _layout++;
                    UpdateButtons(_layout);
                    Refresh();
                }
            }
            else if (e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1)
            {
                FocusLayout(0);
            }
            else if (e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2)
            {
                FocusLayout(1);
            }
            else if (e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3)
            {
                FocusLayout(2);
            }
            else if (e.KeyCode == Keys.NumPad4 || e.KeyCode == Keys.D4)
            {
                FocusLayout(3);
            }
            else if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.D5)
            {
                FocusLayout(4);
            }
            else if (e.KeyCode == Keys.NumPad6 || e.KeyCode == Keys.D6)
            {
                FocusLayout(5);
            }
            else if (e.KeyCode == Keys.NumPad7 || e.KeyCode == Keys.D7)
            {
                FocusLayout(6);
            }
            else if (e.KeyCode == Keys.NumPad8 || e.KeyCode == Keys.D8)
            {
                FocusLayout(7);
            }
        }

        private void SelectLayout(int layout)
        {
            if (_loading)
            {
                return;
            }

            _layout = layout;
            UpdateButtons(_layout);
            Refresh();
        }

        private void button1_Enter(object sender, EventArgs e)
        {
            SelectLayout(0);
        }


        private void button2_Enter(object sender, EventArgs e)
        {
            SelectLayout(1);
        }

        private void button3_Enter(object sender, EventArgs e)
        {
            SelectLayout(2);
        }

        private void button4_Enter(object sender, EventArgs e)
        {
            SelectLayout(3);
        }

        private void button5_Enter(object sender, EventArgs e)
        {
            SelectLayout(4);
        }

        private void button6_Enter(object sender, EventArgs e)
        {
            SelectLayout(5);
        }

        private void button7_Enter(object sender, EventArgs e)
        {
            SelectLayout(6);
        }

        private void button8_Enter(object sender, EventArgs e)
        {
            SelectLayout(7);
        }

        private void button9_Enter(object sender, EventArgs e)
        {
            SelectLayout(8);
        }

        private void LayoutPicker_Shown(object sender, EventArgs e)
        {
            FocusLayout();
            _loading = false;
            SelectLayout(_layout);
        }

        private void FocusLayout(int layout)
        {
            _layout = layout;
            FocusLayout();
        }

        private void FocusLayout()
        {
            switch (_layout)
            {
                case 0:
                    button1.Focus();
                    break;
                case 1:
                    button2.Focus();
                    break;
                case 2:
                    button3.Focus();
                    break;
                case 3:
                    button4.Focus();
                    break;
                case 4:
                    button5.Focus();
                    break;
                case 5:
                    button6.Focus();
                    break;
                case 6:
                    button7.Focus();
                    break;
                case 7:
                    button8.Focus();
                    break;
                case 8:
                    button9.Focus();
                    break;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void checkBoxHideVideoControls_CheckedChanged(object sender, EventArgs e)
        {
            ShowVideoControls = checkBoxHideVideoControls.Checked;
        }
    }
}
