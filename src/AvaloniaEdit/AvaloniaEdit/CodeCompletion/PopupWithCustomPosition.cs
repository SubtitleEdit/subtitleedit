using Avalonia;
using Avalonia.Controls.Primitives;

namespace AvaloniaEdit.CodeCompletion
{
    internal class PopupWithCustomPosition : Popup
    {
        public Point Offset
        {
            get
            {
                return new Point(HorizontalOffset, VerticalOffset);
            }
            set
            {
                HorizontalOffset = value.X;
                VerticalOffset = value.Y;

                //this.Revalidate(VerticalOffsetProperty);
            }
        }
    }
}