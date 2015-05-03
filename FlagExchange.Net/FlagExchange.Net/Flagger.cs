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
using System.Net;
using System.Text.RegularExpressions;
using CsQuery;



namespace FlagExchangeDotNet
{
    public class Flagger
    {
        private readonly Regex hostParser = new Regex(@".*//|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex postIDParser1 = new Regex(@"\D*/|\D.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex postIDParser2 = new Regex(@".*(q|a)/|/\d*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex isShareLink = new Regex(@"(q|a)/\d*/\d*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly string email;
        private readonly string password;
        private readonly string name;
        private readonly List<string> loggedInSites = new List<string>();



        public Flagger(string name, string email, string password)
        {
            if (String.IsNullOrEmpty(email)) { throw new ArgumentException("'email' must not be null or empty.", "email"); }
            if (String.IsNullOrEmpty(password)) { throw new ArgumentException("'password' must not be null or empty.", "password"); }

            this.email = email;
            this.password = password;
            this.name = name;
        }



        public bool FlagSpam(string postUrl)
        {
            string host;
            int postID;

            GetFlagData(postUrl, out host, out postID);

            if (!loggedInSites.Contains(host))
            {
                SiteSignupLogin(host);
                loggedInSites.Add(host);
            }

            var fkey = CQ.Create(RequestManager.GetResponseContent(RequestManager.SendGETRequest("https://" + host + "/users/signup"))).GetFkey();

            var res = RequestManager.SendPOSTRequest("http://" + host + "/flags/posts/" + postID + "/add/PostSpam", "fkey=" + fkey + "&otherText=");

            return FlagWasSucessful(res);
        }

        public bool FlagOffensive(string postUrl)
        {
            string host;
            int postID;

            GetFlagData(postUrl, out host, out postID);

            if (!loggedInSites.Contains(host))
            {
                SiteSignupLogin(host);
                loggedInSites.Add(host);
            }

            var fkey = CQ.Create(RequestManager.GetResponseContent(RequestManager.SendGETRequest("https://" + host + "/users/signup"))).GetFkey();

            var res = RequestManager.SendPOSTRequest("http://" + host + "/flags/posts/" + postID + "/add/PostOffensive", "fkey=" + fkey + "&otherText=");

            return FlagWasSucessful(res);
        }



        private bool FlagWasSucessful(HttpWebResponse res)
        {
            if (res == null) { return false; }

            var resContent = RequestManager.GetResponseContent(res);

            return resContent.StartsWith("{\"Success\":true");
        }

        private void GetFlagData(string postUrl, out string host, out int postID)
        {
            host = hostParser.Replace(postUrl, "");

            if (isShareLink.IsMatch(postUrl))
            {
                postID = int.Parse(postIDParser2.Replace(postUrl, ""));
            }
            else
            {
                postID = int.Parse(postIDParser1.Replace(postUrl, ""));
            }
        }



        private void SiteSignupLogin(string host)
        {
            var e = Uri.EscapeDataString(email);
            var p = Uri.EscapeDataString(password);
            var n = Uri.EscapeDataString(name);
            var referrer = "https://" + host + "/users/signup?returnurl=http://" + host + "%2f";
            var origin = "https://" + host + ".com";
            var fkey = CQ.Create(RequestManager.GetResponseContent(RequestManager.SendGETRequest("https://" + host + "/users/signup"))).GetFkey();

            var data = "fkey=" + fkey + "&display-name=" + n + "&email=" + e + "&password=" + p + "&password2=" + p + "&legalLinksShown=1";

            var res = RequestManager.SendPOSTRequest("https://" + host + "/users/signup", data, true, referrer, origin);

            if (res == null) { throw new Exception("Could not login/sign-up."); }

            var resContent = RequestManager.GetResponseContent(res);

            // We already have an account (and we've been logged in).
            if (!resContent.Contains("We will automatically link this account with your accounts on other Stack Exchange sites.")) { return; }

            // We don't have an account, so lets create one!

            var s = CQ.Create(resContent).GetS();

            res = RequestManager.SendPOSTRequest("https://" + host + "/users/openidconfirm", "fkey=" + fkey + "&s=" + s + "&legalLinksShown=1", true, referrer, origin);

            if (res == null) { throw new Exception("Could not login/sign-up."); }
        }
    }
}
