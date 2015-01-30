using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Linq;
using CsQuery;



namespace FlagExchangeDotNet
{
    public static class Extensions
    {
        public static List<Cookie> GetCookies(this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, container, new object[] { });

            foreach (var key in table.Keys)
            {
                Uri uri;

                var domain = key as string;

                if (domain == null) { continue; }

                if (domain.StartsWith("."))
                {
                    domain = domain.Substring(1);
                }

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false) { continue; }

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }

        public static string GetFkey(this CQ input)
        {
            var fkeyE = input["input"].First(e => e.Attributes["name"] != null && e.Attributes["name"] == "fkey");

            return fkeyE == null ? "" : fkeyE.Attributes["value"];
        }

        public static string GetS(this CQ input)
        {
            var sE = input["input"].First(e => e.Attributes["name"] != null && e.Attributes["name"] == "s");

            return sE == null ? "" : sE.Attributes["value"];
        }
    }

}
