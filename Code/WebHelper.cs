using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Code {
    class WebHelper {
        public bool GetHtmlFromUrl(string url, ParserProcess Caller, ref DateTime lastRequest, TimeSpan requestFrequency, out string Result) {
            Result = "";
            try {
                Random Rnd = new Random();

                while (DateTime.Now < lastRequest + requestFrequency.Add(new TimeSpan(0, 0, 0, 0, Rnd.Next(1, 3999)))) {
                    Thread.Sleep(1000);
                }
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
                Request.ProtocolVersion = HttpVersion.Version11;
                Request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
                Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*;q=0.8";
                Request.Headers.Set("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
                Request.Headers.Set("Accept-Encoding", "gzip, deflate, sdch");

                Request.Method = "GET";
                Request.Timeout = 10000;
                using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse()) {
                    Caller.FireNewHttpRequest();
                    // записали время последнего запроса
                    lastRequest = DateTime.Now;
                    if (Response.StatusCode == HttpStatusCode.OK) {
                        Stream receiveStream = Response.GetResponseStream();
                        GZipStream CompressedStream = new GZipStream(receiveStream, CompressionMode.Decompress);

                        StreamReader readStream = null;

                        if (Response.CharacterSet == null) {
                            readStream = new StreamReader(CompressedStream);
                        }
                        else {
                            readStream = new StreamReader(CompressedStream, Encoding.GetEncoding(Response.CharacterSet));
                        }

                        Result = readStream.ReadToEnd();

                        Response.Close();
                        readStream.Close();
                    }
                    else {
                        return false;
                    }
                }
                return true;
            }
            catch {

            }
            return false;
        }
        //public string GetHtmlFromPhantom(string url, ParserProcess Caller, ref DateTime lastRequest, TimeSpan requestFrequency) {
        //    Random Rnd = new Random();
        //    TimeSpan NewTs = requestFrequency.Add(new TimeSpan(0, 0, Rnd.Next(1, 5)));
        //    if (DateTime.Now< lastRequest + NewTs) {
        //        Thread.Sleep(((lastRequest + NewTs) - DateTime.Now).Milliseconds);
        //    }

        //    string arguments = "pjs\\n " + url;

        //    Process p = new Process();
        //    p.StartInfo.UseShellExecute = false;
        //    p.StartInfo.RedirectStandardOutput = true;
        //    p.StartInfo.CreateNoWindow = true;
        //    p.StartInfo.FileName = "pjs\\phantomjs.exe";
        //    p.StartInfo.Arguments = arguments;
        //    p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        //    p.Start();

        //    Caller.FireNewHttpRequest();

        //    string output = p.StandardOutput.ReadToEnd();
        //    p.WaitForExit();

        //    return output;
        //}
    }
}
