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

namespace Phamhilator.Pham.Core
{
    public class ReportedUser
    {
        private readonly DateTime creationTime;
        private readonly string site;
        private readonly string name;

        public string Site
        {
            get
            {
                // Invalidate data after 5 hours.
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 300)
                {
                    return "";
                }

                return site;
            }
        }

        public string Name
        {
            get
            {
                // Invalidate data after 5 hours.
                if ((DateTime.UtcNow - creationTime).TotalMinutes > 300)
                {
                    return "";
                }

                return name;
            }
        }



        public ReportedUser(string site, string name)
        {
            creationTime = DateTime.UtcNow;

            this.site = site;
            this.name = name;
        }
    }
}