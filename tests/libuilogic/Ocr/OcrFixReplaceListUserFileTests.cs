using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System;
using System.IO;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// OCR replace lists are named after the three-letter ISO code ("ell_OCRFixReplaceList.xml"), and a
/// user's own additions live in the "_User" sibling ("ell_OCRFixReplaceList_User.xml") - which must
/// be honored even when the shipped list does not exist at all (issue #13814).
/// </summary>
public class OcrFixReplaceListUserFileTests : IDisposable
{
    private readonly string _folder;

    public OcrFixReplaceListUserFileTests()
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

    private string Write(string fileName, string xml)
    {
        var path = Path.Combine(_folder, fileName);
        File.WriteAllText(path, xml);
        return path;
    }

    [Fact]
    public void UserFileName_IsTheUserSibling()
    {
        var fileName = Path.Combine(_folder, "ell_OCRFixReplaceList.xml");

        Assert.Equal(Path.Combine(_folder, "ell_OCRFixReplaceList_User.xml"),
            OcrFixReplaceList2.GetUserFileName(fileName));
    }

    [Fact]
    public void UserListAlone_IsLoaded()
    {
        // No shipped list at all - just the user's own file.
        Write("ell_OCRFixReplaceList_User.xml",
            "<ReplaceList><WholeWords><Word from=\"Teh\" to=\"The\" /></WholeWords></ReplaceList>");

        var list = new OcrFixReplaceList2(Path.Combine(_folder, "ell_OCRFixReplaceList.xml"));

        Assert.Equal("The", list.WordReplaceList["Teh"]);
    }

    [Fact]
    public void UserList_AddsToAndRemovesFromTheShippedList()
    {
        Write("eng_OCRFixReplaceList.xml",
            "<ReplaceList><WholeWords><Word from=\"Teh\" to=\"The\" /><Word from=\"Iine\" to=\"line\" /></WholeWords></ReplaceList>");
        Write("eng_OCRFixReplaceList_User.xml",
            "<ReplaceList><WholeWords><Word from=\"Ivlan\" to=\"Man\" /></WholeWords>" +
            "<RemovedWholeWords><Word from=\"Teh\" to=\"\" /></RemovedWholeWords></ReplaceList>");

        var list = new OcrFixReplaceList2(Path.Combine(_folder, "eng_OCRFixReplaceList.xml"));

        Assert.Equal("line", list.WordReplaceList["Iine"]);
        Assert.Equal("Man", list.WordReplaceList["Ivlan"]);
        Assert.False(list.WordReplaceList.ContainsKey("Teh"));
    }

    [Fact]
    public void AddWordOrPartial_WritesTheUserFile()
    {
        var list = new OcrFixReplaceList2(Path.Combine(_folder, "eng_OCRFixReplaceList.xml"));

        Assert.True(list.AddWordOrPartial("Teh", "The"));

        var reloaded = new OcrFixReplaceList2(Path.Combine(_folder, "eng_OCRFixReplaceList.xml"));
        Assert.Equal("The", reloaded.WordReplaceList["Teh"]);
        Assert.True(File.Exists(Path.Combine(_folder, "eng_OCRFixReplaceList_User.xml")));
        Assert.False(File.Exists(Path.Combine(_folder, "eng_OCRFixReplaceList.xml")));
    }

    // The Greek list keeps its own letters: FixCommonWordErrors maps U+03BD GREEK SMALL LETTER NU
    // to "v" for every other language. That check used to look for a hard-coded "\\ell...", so it
    // never matched on macOS or Linux and Greek text lost its nu there.
    [Theory]
    [InlineData("ell_OCRFixReplaceList.xml", "νν")]
    [InlineData("eng_OCRFixReplaceList.xml", "vv")]
    public void GreekListKeepsItsNu_WhateverTheSeparator(string fileName, string expected)
    {
        Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
        var list = new OcrFixReplaceList2(Path.Combine(_folder, fileName));

        Assert.Equal(expected, list.FixCommonWordErrors("νν"));
    }
}
