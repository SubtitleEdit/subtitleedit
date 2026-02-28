namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class MouseStatus
{
    public bool MouseButton1 { get; set; }
    public bool MouseButton2 { get; set; }
    public bool MouseButtonNone => !MouseButton1 && !MouseButton2;
}