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