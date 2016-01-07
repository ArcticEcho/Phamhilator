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





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Phamhilator.Pham
{
    class ConfigWriter
    {
        public void UpdateSetting(string settingName, string data)
        {
            var st = settingName.ToLowerInvariant();
            var dataz = File.ReadAllLines("settings.txt");

            if (dataz.All(x => !x.Contains(settingName)))
            {
                File.AppendAllText("settings.txt", Environment.NewLine + settingName + ":" + data);
            }
            else
            {
                var newLines = new List<string>();

                foreach (var line in dataz)
                {
                    var l = line;

                    if (l.ToLowerInvariant().StartsWith(st))
                    {
                        l = settingName + ":" + data;
                    }

                    newLines.Add(l);
                }

                File.WriteAllLines("settings.txt", newLines);
            }
        }
    }
}
