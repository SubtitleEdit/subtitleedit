using Nikse.SubtitleEdit.Features.Edit.Replace;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public interface IFindResult
{
    public void RequestFindData();
    public void HandleFindResult(FindViewModel result);
    public void HandleReplaceResult(ReplaceViewModel result);
}
