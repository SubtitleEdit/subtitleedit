using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace LibSETests.Dictionaries;

[Collection("NonParallelTests")]
public class XmlDictionariesTest
{
    [Fact]
    public void DictionariesValidReplaceListRegEx()
    {
        var xmlFileNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "???_OCRFixReplaceList.xml");
        foreach (var xmlFileName in xmlFileNames)
        {
            try
            {
                var doc = XDocument.Load(xmlFileName);
                var regExList = from regEx in doc.Descendants("RegularExpressions").DescendantNodes() select regEx;
                foreach (var xNode in regExList)
                {
                    var e = xNode as XElement;
                    if (e != null && e.NodeType == XmlNodeType.Element && e.Attribute("find") != null)
                    {
                        var find = e.Attribute("find");
                        var regExPattern =  e.Value;
                        try
                        {
                            new Regex(regExPattern);
                        }
                        catch (Exception exception)
                        {
                            string msg = Path.GetFileName(xmlFileName) + " has an invalid RegEx find expression: " + regExPattern + Environment.NewLine +
                                         exception.Message;
                            Assert.Fail(msg);
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
