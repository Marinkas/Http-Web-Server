using System;
using System.Collections.Generic;
using System.Text;

namespace HttpWebServer
{
    public class Response
    {
        public string Version { get; private set; }
        public string Status { get; set; }

        public Dictionary<string, string> Headers { get; private set; }

        public byte[] Data { get; set; }

        public Response()
        {
            Version = "HTTP/1.1";
            Status = "200 OK";

            Headers = new Dictionary<string, string>();
        }

        public byte[] GetBytes()
        {
            string HeaderStr = String.Format("{0} {1}\r\n", Version, Status);

            Console.WriteLine("Version: {0} Status: {1}", Version, Status);

            foreach (KeyValuePair<string, string> Header in Headers)
            {
                HeaderStr = String.Format("{0}{1}: {2}\r\n", HeaderStr, Header.Key, Header.Value);
            }
            HeaderStr += "\r\n";

            byte[] HeaderDat = Encoding.UTF8.GetBytes(HeaderStr);
            byte[] ReturnBytes = Program.Combine(HeaderDat, Data);

            return ReturnBytes;
        }
    }
}
