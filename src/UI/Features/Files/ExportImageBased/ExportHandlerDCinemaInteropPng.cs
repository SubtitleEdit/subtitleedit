using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerDCinemaInteropPng : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.DCinemaPng;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "DCinema interop/png");

    private StringBuilder _sb = new StringBuilder();
    private string _folderName = string.Empty;
    private int _imagesSavedCount = 0;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _sb.Clear();
        _imagesSavedCount = 0;
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        _imagesSavedCount++;
        string numberString = $"{_imagesSavedCount:0000}";
        string fileName = Path.Combine(_folderName, numberString + ".png");
        File.WriteAllBytes(fileName, param.Bitmap.ToPngArray());

        string verticalAlignment = "bottom";
        string horizontalAlignment = "center";
        string vPos = "9.7";
        string hPos = "0";

        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                verticalAlignment = "bottom";
                horizontalAlignment = "left";
                hPos = "10";
                break;
            case ExportAlignment.BottomRight:
                verticalAlignment = "bottom";
                horizontalAlignment = "right";
                hPos = "10";
                break;
            case ExportAlignment.MiddleCenter:
                verticalAlignment = "center";
                vPos = "0";
                break;
            case ExportAlignment.MiddleLeft:
                verticalAlignment = "center";
                horizontalAlignment = "left";
                hPos = "10";
                vPos = "0";
                break;
            case ExportAlignment.MiddleRight:
                verticalAlignment = "center";
                horizontalAlignment = "right";
                hPos = "10";
                vPos = "0";
                break;
            case ExportAlignment.TopCenter:
                verticalAlignment = "top";
                break;
            case ExportAlignment.TopLeft:
                verticalAlignment = "top";
                horizontalAlignment = "left";
                hPos = "10";
                break;
            case ExportAlignment.TopRight:
                verticalAlignment = "top";
                horizontalAlignment = "right";
                hPos = "10";
                break;
        }

        _sb.AppendLine("<Subtitle FadeDownTime=\"" + 0 + "\" FadeUpTime=\"" + 0 + "\" TimeOut=\"" + DCinemaInterop.ConvertToTimeString(new TimeCode(param.EndTime)) +
                       "\" TimeIn=\"" +
                       DCinemaInterop.ConvertToTimeString(new TimeCode(param.StartTime)) + "\" SpotNumber=\"" + _imagesSavedCount + "\">");
        // if (param.Depth3D == 0)
        {
            _sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" +
                           numberString + ".png" + "</Image>");
        }
        // else
        // {
        //     sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" ZPosition=\"" + param.Depth3D + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" +
        //                   horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
        // }

        _sb.AppendLine("</Subtitle>");
    }

    public void WriteFooter()
    {
        var doc = new XmlDocument();
        string title = Path.GetFileNameWithoutExtension(_folderName);

        string guid = Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                    "<DCSubtitle Version=\"1.1\">" + Environment.NewLine +
                    "<SubtitleID>" + guid + "</SubtitleID>" + Environment.NewLine +
                    "<MovieTitle>" + title + "</MovieTitle>" + Environment.NewLine +
                    "<ReelNumber>1</ReelNumber>" + Environment.NewLine +
                    "<Language>English</Language>" + Environment.NewLine +
                    _sb +
                    "</DCSubtitle>");
        string fName = Path.Combine(_folderName, "index.xml");

        File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
    }
}