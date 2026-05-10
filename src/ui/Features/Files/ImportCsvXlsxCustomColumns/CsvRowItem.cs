using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ImportCsvXlsxCustomColumns;

public class CsvRowItem
{
    public List<string> Cells { get; }

    public CsvRowItem(List<string> cells)
    {
        Cells = cells;
    }
}
