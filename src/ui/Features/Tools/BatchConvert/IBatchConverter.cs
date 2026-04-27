using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public interface IBatchConverter
{
    void Initialize(BatchConvertConfig config); 
    Task Convert(BatchConvertItem item, System.Threading.CancellationToken cancellationToken);
}