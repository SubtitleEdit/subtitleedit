using Nikse.SubtitleEdit.Core.Common;

namespace Tests.Logic
{
    
    public class HtmlUtilTest
    {
        [Fact]
        public void GetColorFromString1()
        {
            var c = HtmlUtil.GetColorFromString("#010203ff");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void GetColorFromString2()
        {
            var c = HtmlUtil.GetColorFromString("rgb(1,2,3)");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void GetColorFromString3()
        {
            var c = HtmlUtil.GetColorFromString("rgba(1,2,3, 1)");

            Assert.Equal(byte.MaxValue, c.Alpha);
            Assert.Equal(1, c.Red);
            Assert.Equal(2, c.Green);
            Assert.Equal(3, c.Blue);
        }

        [Fact]
        public void RemoveFontName1()
        {
            var c = HtmlUtil.RemoveFontName(@"{\fnVerdena}Hallo!");

            Assert.Equal("Hallo!", c);
        }

        [Fact]
        public void RemoveFontName2()
        {
            var c = HtmlUtil.RemoveFontName(@"{\an2\fnVerdena}Hallo!");
            Assert.Equal(@"{\an2}Hallo!", c);
        }

        [Fact]
        public void RemoveFontName3()
        {
            var c = HtmlUtil.RemoveFontName(@"{\an2\fnVerdena\i1}Hallo!");
            Assert.Equal(@"{\an2\i1}Hallo!", c);
        }

        [Fact]
        public void RemoveFontName4()
        {
            var c = HtmlUtil.RemoveFontName("<font face=\"Verdena\">Hallo!</font>");

            Assert.Equal("Hallo!", c);
        }

        [Fact]
        public void IsTextFormattableFalse()
        {
            Assert.False(HtmlUtil.IsTextFormattable("<i></i>"));
            Assert.False(HtmlUtil.IsTextFormattable("<i>!?."));
            Assert.False(HtmlUtil.IsTextFormattable("!?.</i>"));
        }

        [Fact]
        public void IsTextFormattableTrue()
        {
            Assert.True(HtmlUtil.IsTextFormattable("<i>a</i>"));
            Assert.True(HtmlUtil.IsTextFormattable("<u>1</u>"));
            Assert.True(HtmlUtil.IsTextFormattable("<i>A</i>"));
            Assert.True(HtmlUtil.IsTextFormattable("</i")); // invalid closing tag
        }
    }
}