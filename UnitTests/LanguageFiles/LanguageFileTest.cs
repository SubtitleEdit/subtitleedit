using System.Xml;

namespace Tests.LanguageFiles
{
    
    public class LanguageFileTest
    {
        [Fact]
        public void TestLanguageFiles()
        {
            var idx = Environment.CurrentDirectory.IndexOf(Path.DirectorySeparatorChar + "subtitleedit" + Path.DirectorySeparatorChar);
            var baseDir = Environment.CurrentDirectory.Substring(0, idx);
            var languageDir = Path.Combine(baseDir, "subtitleedit", "src", "ui", "Languages");
            var files = Directory.GetFiles(languageDir, "*.xml");

            foreach (var fileName in files)
            {
                var xml = new XmlDocument();
                try
                {
                    xml.Load(fileName);
                    if (xml.DocumentElement == null)
                    {
                        throw new Exception("DocumentElement is null");
                    }
                    var node = xml.DocumentElement.SelectSingleNode("General/CultureName");
                    if (node == null)
                    {
                        throw new Exception("CultureName node is null");
                    }
                    var cultureName = node.InnerText;
                    var fileNameStart = Path.GetFileName(fileName).Substring(0, cultureName.Length);
                    Assert.Equal(fileNameStart, cultureName);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Unable to load language file {fileName} : {exception.Message}");
                }
            }
        }
    }
}
