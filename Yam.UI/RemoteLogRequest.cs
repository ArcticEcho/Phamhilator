﻿/*
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





using System;

namespace Phamhilator.Yam.UI
{
    public class RemoteLogRequest
    {
        public string SearchBy { get; set; }
        public string SearchPattern { get; set; }
        public string PostType { get; set; }
        public DateTime StartCreationDate { get; set; }
        public DateTime EndCreationDate { get; set; }
        public DateTime StartEntryDate { get; set; }
        public DateTime EndEntryDate { get; set; }
        public string Site { get; set; }



        public RemoteLogRequest()
        {
            StartCreationDate = DateTime.MinValue;
            EndCreationDate = DateTime.MaxValue;
            StartEntryDate = DateTime.MinValue;
            EndEntryDate = DateTime.MaxValue;
        }
    }
}
