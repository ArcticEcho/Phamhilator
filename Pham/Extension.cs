/*
 * Phamhilator.A.Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */





namespace Phamhilator.Pham.UI
{
    public static class Extension
    {
        public static bool ContainsCodeBlockTag(this string[] tags)
        {
            foreach (var t in tags)
            {
                if (t == "•CB-S•" || t == "•CB-M•" || t == "•CB-L•")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsBlockQuoteTag(this string[] tags)
        {
            foreach (var t in tags)
            {
                if (t == "•BQ-S•" || t == "•BQ-M•" || t == "•BQ-L•")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsInlineCodeTag(this string[] tags)
        {
            foreach (var t in tags)
            {
                if (t == "•IC-S•" || t == "•IC-M•" || t == "•IC-L•")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsLinkTag(this string[] tags)
        {
            foreach (var t in tags)
            {
                if (t == "•L•")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsPictureTag(this string[] tags)
        {
            foreach (var t in tags)
            {
                if (t == "•P•")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
