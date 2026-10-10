using System.Xml;

namespace Nikse.SubtitleEdit.UiLogic.SpellCheck;

/// <summary>
/// Reads and writes the spell check "use always" list (&lt;lang&gt;_UseAlways.xml): the pairs stored by
/// "Change all" / "Use always" that replace a misspelled word automatically. Shared by the spell
/// checker and the list editor.
/// </summary>
public static class UseAlwaysListFile
{
    public static string GetFileName(string dictionaryFolder, string languageName)
    {
        return Path.Combine(dictionaryFolder, languageName + "_UseAlways.xml");
    }

    public static Dictionary<string, string> Load(string fileName)
    {
        var result = new Dictionary<string, string>();
        if (!File.Exists(fileName))
        {
            return result;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(fileName);
        var xmlNodeList = xmlDoc.DocumentElement?.SelectNodes("Pair");
        if (xmlNodeList == null)
        {
            return result;
        }

        foreach (XmlNode item in xmlNodeList)
        {
            var from = item.Attributes?["from"]?.Value;
            var to = item.Attributes?["to"]?.Value;
            if (to != null && from != null && !result.ContainsKey(from))
            {
                result.Add(from, to);
            }
        }

        return result;
    }

    public static void Save(string fileName, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<UseAlways></UseAlways>");
        foreach (var kvp in pairs)
        {
            XmlNode node = xmlDoc.CreateElement("Pair");
            var f = xmlDoc.CreateAttribute("from");
            f.Value = kvp.Key;
            var t = xmlDoc.CreateAttribute("to");
            t.Value = kvp.Value;
            node.Attributes?.Append(f);
            node.Attributes?.Append(t);
            xmlDoc.DocumentElement?.AppendChild(node);
        }

        xmlDoc.Save(fileName);
    }
}
