/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */






using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phamhilator.Yam.Core;

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
            StringAssert.Matches(longUrl, new Regex(@"^http\:\/\/www\.optimalstackfacts\.org\/$"));
        }
    }
}
