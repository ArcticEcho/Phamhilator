using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JsonFx.Json;
using JsonFx.Serialization;



namespace Phamhilator.Core
{
    public static class Hastebin
    {
        public static string PostDocument(string documentContent)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://hastebin.com/documents");
            var data = Encoding.UTF8.GetBytes(documentContent);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            dynamic resJson = new JsonReader().Read(new StreamReader(response.GetResponseStream()).ReadToEnd());

            return "http://hastebin.com/" + (string)resJson.key + ".hs";
        }
    }
}
