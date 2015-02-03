using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phamhilator.Core;

namespace Phamhilator.Tests
{
    [TestClass]
    public class ActiveRoomsTest
    {
        [TestMethod]
        public void PrimaryRoomUrlTest()
        {
            var activeRooms = new ActiveRooms();
            var primaryRoomUrl = activeRooms.PrimaryRoomUrl;

            StringAssert.StartsWith(primaryRoomUrl, "http");
        }

        [TestMethod]
        public void SecondaryRoomUrlsTest()
        {
            var activeRooms = new ActiveRooms();
            var secRoomUrls = activeRooms.SecondaryRoomUrls;

            Assert.IsNotNull(secRoomUrls);
            Assert.IsTrue(secRoomUrls.Count >= 1);
            CollectionAssert.AllItemsAreNotNull(secRoomUrls);

            foreach (var url in secRoomUrls)
            {
                StringAssert.StartsWith(url, "http");
            }
        }
    }
}
