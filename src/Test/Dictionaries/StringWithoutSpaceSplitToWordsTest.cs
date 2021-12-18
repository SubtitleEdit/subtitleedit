using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Dictionaries;
using System;
using System.Linq;

namespace Test.Dictionaries
{
    [TestClass]
  public class StringWithoutSpaceSplitToWordsTest
  {
    [TestMethod]
    public void DictionariesValidXml()
    {
        var words = "we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(p => p.Length).ToArray();
        var input = "wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovideforthecommondefencepromotethegeneralwelfareandsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica";
        var result = StringWithoutSpaceSplitToWords.SplitWord(words, input);
        Assert.AreEqual("we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america", result);
    }
  }
}