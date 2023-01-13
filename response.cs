using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            string headerStr = String.Format("{0} {1}\r\n", Version, Status);

            Console.WriteLine("Version: {0} Status: {1}", Version, Status);

            foreach (KeyValuePair<string, string> Header in Headers)
            {
                headerStr = String.Format("{0}{1}: {2}\r\n", headerStr, Header.Key, Header.Value);
            }
            headerStr += "\r\n";

            byte[] headerBytes = Encoding.UTF8.GetBytes(headerStr);
            byte[] responsBytes = new byte[headerBytes.Length + this.Data.Length];

            // combine data
            Buffer.BlockCopy(headerBytes, 0, responsBytes, 0, headerBytes.Length);
            Buffer.BlockCopy(this.Data, 0, responsBytes, headerBytes.Length, this.Data.Length);

            return responsBytes;
        }
    }
}
