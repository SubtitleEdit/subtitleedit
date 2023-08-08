using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.SeMsgBox
{
    public static class MessageBox
    {
        public static DialogResult Show(string text)
        {
            using (var msgBox = new MessageBoxForm(text, string.Empty, MessageBoxButtons.OK))
            {
                return msgBox.ShowDialog(Form.ActiveForm);
            }
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            using (var msgBox = new MessageBoxForm(text, caption, buttons))
            {
               return msgBox.ShowDialog(Form.ActiveForm);
            }
        }

        public static DialogResult Show(Form form, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var msgBox = new MessageBoxForm(text, caption, buttons))
            {
                return msgBox.ShowDialog(form);
            }
        }

        internal static DialogResult Show(Form form, string text, string title, MessageBoxButtons buttons)
        {
            using (var msgBox = new MessageBoxForm(text, title, buttons))
            {
                return msgBox.ShowDialog(form);
            }
        }

        internal static DialogResult Show(string text, string caption)
        {
            using (var msgBox = new MessageBoxForm(text, caption, MessageBoxButtons.OK))
            {
                return msgBox.ShowDialog(Form.ActiveForm);
            }
        }

        internal static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var msgBox = new MessageBoxForm(text, title, buttons))
            {
                return msgBox.ShowDialog(Form.ActiveForm);
            }
        }

        internal static DialogResult Show(Form form, string text, string caption)
        {
            using (var msgBox = new MessageBoxForm(text, caption, MessageBoxButtons.OK))
            {
                return msgBox.ShowDialog(Form.ActiveForm);
            }
        }
    }
}
