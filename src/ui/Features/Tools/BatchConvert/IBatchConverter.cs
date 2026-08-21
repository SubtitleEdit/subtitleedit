using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public interface IBatchConverter
{
    void Initialize(BatchConvertConfig config); 
    Task Convert(BatchConvertItem item, System.Threading.CancellationToken cancellationToken);

    /// <summary>
    /// True once a convert in this converter's lifetime has put the SE-managed llama-server to
    /// work for OCR. The view model's cancel/close shutdown keys on it, so a batch whose OCR
    /// engine is merely *configured* as llama.cpp - but which never OCRs anything - does not
    /// kill a server another window started (#13865).
    /// </summary>
    bool UsedLocalLlamaCppOcr { get; }
}