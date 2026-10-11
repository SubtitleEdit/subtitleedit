using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>Another OCR engine that "Find errors in nOCR database" compares nOCR with.</summary>
public partial class OcrCompareEngine : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private bool _isEnabled = true;

    public string Name { get; }

    /// <summary>Reads a batch of images; returns the text per image index (missing = engine failed).</summary>
    public Func<IReadOnlyList<(int Index, SKBitmap Bitmap)>, CancellationToken, Task<Dictionary<int, string>>> Read { get; }

    public OcrCompareEngine(string name, Func<IReadOnlyList<(int Index, SKBitmap Bitmap)>, CancellationToken, Task<Dictionary<int, string>>> read)
    {
        Name = name;
        Read = read;
    }

    public override string ToString() => Name;
}
