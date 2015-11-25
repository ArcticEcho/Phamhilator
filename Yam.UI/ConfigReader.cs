using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phamhilator.Yam.UI
{
    class ConfigReader
    {
        public string GetSetting(string settingName)
        {
            var st = settingName.ToLowerInvariant();
            var dataz = File.ReadAllLines("Settings.txt");

            foreach (var line in dataz)
            {
                if (line.ToLowerInvariant().StartsWith(st))
                {
                    return line.Remove(0, line.IndexOf(":") + 1);
                }
            }

            return null;
        }
    }
}
