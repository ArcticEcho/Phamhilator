using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phamhilator.Core;



namespace Phamhilator.Tests
{
    [TestClass]
    public class LinkUnshortifierTest
    {
        [TestMethod]
        public void IsShortLinkTest()
        {
            var isShort = LinkUnshortifier.IsShortLink(null);
            Assert.IsFalse(isShort);

            isShort = LinkUnshortifier.IsShortLink("");
            Assert.IsFalse(isShort);

            isShort = LinkUnshortifier.IsShortLink("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
            Assert.IsFalse(isShort);

            isShort = LinkUnshortifier.IsShortLink("http://goo.gl/yvgSRd");
            Assert.IsTrue(isShort);
        }

        [TestMethod]
        public void UnshortifyLinkTest()
        {
            var longUrl = LinkUnshortifier.UnshortifyLink(null);
            Assert.IsNull(longUrl);

            longUrl = LinkUnshortifier.UnshortifyLink("");
            Assert.AreSame("", longUrl);

            longUrl = LinkUnshortifier.UnshortifyLink("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
            Assert.AreSame("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq", longUrl);

            longUrl = LinkUnshortifier.UnshortifyLink("http://goo.gl/yvgSRd");
            Assert.AreSame("http://www.optimalstackfacts.org/", longUrl);
        }
    }
}
