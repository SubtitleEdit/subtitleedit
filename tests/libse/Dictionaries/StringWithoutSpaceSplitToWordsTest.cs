using Nikse.SubtitleEdit.Core.Dictionaries;

namespace LibSETests.Dictionaries;

public class StringWithoutSpaceSplitToWordsTest
{
    [Fact]
    public void DictionariesValidXml()
    {
        var words = "we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(p => p.Length).ToArray();
        var input = "wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovideforthecommondefencepromotethegeneralwelfareandsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica";
        var result = StringWithoutSpaceSplitToWords.SplitWord(words, input, "eng");
        Assert.Equal("we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america", result);
    }
}