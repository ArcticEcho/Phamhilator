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





using System.IO;
using System.Net;
using System.Text;
using ServiceStack.Text;

namespace Phamhilator.Yam.Core
{
    public static class Hastebin
    {
        public static string PostDocument(string documentContent)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://hastebin.com/documents");
            var data = Encoding.UTF8.GetBytes(documentContent);

            request.Method = "POST";
            request.Headers["Origin"] = "http://hastebin.com/";
            request.Referer = "http://hastebin.com/";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var content = reader.ReadToEnd();
                var key = JsonObject.Parse(content).Get<string>("key");

                return "http://hastebin.com/" + key + ".hs";
            }
        }
    }
}
