using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Common;

public interface IWaveformContextMenuPlugin
{
    string Name { get; }
    string MenuItemText { get; }
    bool CanExecute(string videoFileName, double startSeconds, double endSeconds);
    Task ExecuteAsync(string videoFileName, double startSeconds, double endSeconds, string outputFolder);
}
