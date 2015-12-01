using System;
using System.Text.RegularExpressions;

namespace Phamhilator.Yam.Core
{
    public static class Extensions
    {
        public static bool IsReDoS(this Regex reg)
        {
            if (reg == null) { return false; }

            var regCpy = new Regex(reg.ToString(), reg.Options, TimeSpan.FromMilliseconds(50));

            try
            {
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadAlpha"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadNum"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadSpec"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadAlphaNum"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadAlphaSpec"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadNumSpec"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadAlphaNumSpec"));
                regCpy.IsMatch(Properties.Resources.ResourceManager.GetString("NewRegexPayloadRealData"));
            }
            catch (Exception)
            {
                // Yes, in this instance we actually want to catch all possible
                // exceptions. Since that's the sole purpose of a ReDoS (to 
                // cause a denial of service by any means possible).
                return true;
            }

            return false;
        }

        public static bool IsValidRegex(this string pattern)
        {
            try
            {
                var reg = new Regex(pattern);
                reg.IsMatch("test");
                return true;
            }
            catch (Exception)
            {
                // If we can't simply create an instance and
                // match a single word, it's probably invalid.
                return false;
            }
        }
    }
}
