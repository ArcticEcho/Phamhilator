﻿using System;
using ChatExchangeDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phamhilator.Core;



namespace Phamhilator.Tests
{
    [TestClass]
    public class CommandProcessorTest
    {
        [TestMethod]
        public void IsValidCommandTest()
        {
            var isVaild = CommandProcessor.IsValidCommand(null, null);
            Assert.IsFalse(isVaild);

            // *Begins adding unit tests to Pham...*
            isVaild = CommandProcessor.IsValidCommand(null, GlobalTestFields.TestRoom[2992220]);
            Assert.IsFalse(isVaild);

            isVaild = CommandProcessor.IsValidCommand(GlobalTestFields.TestRoom, null);
            Assert.IsFalse(isVaild);

            // *Begins adding unit tests to Pham...*
            isVaild = CommandProcessor.IsValidCommand(GlobalTestFields.TestRoom, GlobalTestFields.TestRoom[2992220]);
            Assert.IsFalse(isVaild);

            // >>kill-it-with-no-regrets-for-sure
            isVaild = CommandProcessor.IsValidCommand(GlobalTestFields.TestRoom, GlobalTestFields.TestRoom[2992195]);
            Assert.IsFalse(isVaild);

            // :2992193 tp testing
            isVaild = CommandProcessor.IsValidCommand(GlobalTestFields.TestRoom, GlobalTestFields.TestRoom[2992274]);
            Assert.IsTrue(isVaild);

            // >>auto-b-a-lq-p ^\<p\>[^a-z]*\<\/p\>$
            isVaild = CommandProcessor.IsValidCommand(GlobalTestFields.TestRoom, GlobalTestFields.TestRoom[2990934]);
            Assert.IsTrue(isVaild);
        }
    }
}