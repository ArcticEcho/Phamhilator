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
using System.Text.RegularExpressions;

namespace Phamhilator.Updater
{
    public struct Version
    {
        private static Regex formatCheck = new Regex(@"^\d+\.\d+\.\d+\.\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Patch { get; private set; }



        public Version(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("'version' cannot be null or empty.", "version");
            if (!formatCheck.IsMatch(version)) throw new ArgumentException("'version' is not of a supported format.", "version");

            var split = version.Split('.');

            Major = int.Parse(split[0]);
            Minor = int.Parse(split[1]);
            Build = int.Parse(split[2]);
            Patch = int.Parse(split[3]);
        }



        public static bool MatchesFormat(string version)
        {
            return !string.IsNullOrWhiteSpace(version) && formatCheck.IsMatch(version);
        }

        public override int GetHashCode()
        {
            return Build.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var ver = obj as Version?;
            if (ver == null) return false;

            return ver == this;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Patch}";
        }

        public static bool operator ==(Version x, Version y)
        {
            return x.Major == y.Major && x.Minor == y.Minor && x.Patch == y.Patch;
        }

        public static bool operator !=(Version x, Version y)
        {
            return x.Major != y.Major && x.Minor != y.Minor && x.Patch != y.Patch;
        }

        public static bool operator >(Version x, Version y)
        {
            return x.Build > y.Build;
        }

        public static bool operator <(Version x, Version y)
        {
            return x.Build < y.Build;
        }
    }
}
