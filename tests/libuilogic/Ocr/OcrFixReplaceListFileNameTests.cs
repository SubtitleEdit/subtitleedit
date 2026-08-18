using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;
using System.IO;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Subtitle Edit names OCR replace lists after the three-letter ISO code, but a list a user writes
/// themselves is just as likely to carry the two-letter code ("el_OCRFixReplaceList_User.xml" for
/// Greek). Such a file was ignored by the OCR run and invisible in the word-list editor (issue
/// #13814); it is now used when nothing exists under the canonical name.
/// </summary>
public class OcrFixReplaceListFileNameTests : IDisposable
{
    private readonly string _folder;

    public OcrFixReplaceListFileNameTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-ocrfix-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    private string Touch(string fileName)
    {
        var path = Path.Combine(_folder, fileName);
        File.WriteAllText(path, "<ReplaceList><WholeWords/></ReplaceList>");
        return path;
    }

    [Fact]
    public void NoFileAtAll_KeepsTheCanonicalThreeLetterName()
    {
        // New lists are written under the name SE itself uses.
        var result = OcrFixReplaceList2.GetReplaceListFileName(_folder, "ell");

        Assert.Equal(Path.Combine(_folder, "ell_OCRFixReplaceList.xml"), result);
    }

    [Fact]
    public void TwoLetterUserList_IsFoundForTheThreeLetterLanguageId()
    {
        var expected = Touch("el_OCRFixReplaceList_User.xml");

        var result = OcrFixReplaceList2.GetReplaceListFileName(_folder, "ell");

        // The general file is what the reader takes; its "_User" sibling is the one on disk here.
        Assert.Equal(Path.Combine(_folder, "el_OCRFixReplaceList.xml"), result);
        Assert.True(File.Exists(expected));
    }

    [Fact]
    public void ThreeLetterList_WinsOverATwoLetterOne()
    {
        Touch("el_OCRFixReplaceList_User.xml");
        Touch("ell_OCRFixReplaceList.xml");

        var result = OcrFixReplaceList2.GetReplaceListFileName(_folder, "ell");

        Assert.Equal(Path.Combine(_folder, "ell_OCRFixReplaceList.xml"), result);
    }

    [Fact]
    public void ThreeLetterList_IsFoundForATwoLetterLanguageId()
    {
        Touch("ell_OCRFixReplaceList.xml");

        var result = OcrFixReplaceList2.GetReplaceListFileName(_folder, "el");

        Assert.Equal(Path.Combine(_folder, "ell_OCRFixReplaceList.xml"), result);
    }

    [Fact]
    public void UnknownLanguageId_IsLeftAsWritten()
    {
        var result = OcrFixReplaceList2.GetReplaceListFileName(_folder, "zzz");

        Assert.Equal(Path.Combine(_folder, "zzz_OCRFixReplaceList.xml"), result);
    }

    // The Greek list keeps its own letters: FixCommonWordErrors maps U+03BD GREEK SMALL LETTER NU
    // to "v" for every other language. That check used to look for a hard-coded "\\ell...", so it
    // never matched on macOS or Linux - and it has to accept "el" now that such a list loads too.
    [Theory]
    [InlineData("ell_OCRFixReplaceList.xml", "\u03bd\u03bd")]
    [InlineData("el_OCRFixReplaceList.xml", "\u03bd\u03bd")]
    [InlineData("eng_OCRFixReplaceList.xml", "vv")]
    public void GreekListKeepsItsNu_WhateverTheSeparatorAndSpelling(string fileName, string expected)
    {
        Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
        var list = new OcrFixReplaceList2(Path.Combine(_folder, fileName));

        Assert.Equal(expected, list.FixCommonWordErrors("\u03bd\u03bd"));
    }
}
