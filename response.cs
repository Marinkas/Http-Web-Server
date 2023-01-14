using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HttpWebServer
{
    public class Response
    {
        // status line
        private string HttpVersion;
        private string StatusCode;
        private string ReasonPhrase;

        // the rest of the responce
        private Dictionary<string, string> Headers;
        private byte[] Body;

        public Response()
        {
            this.HttpVersion = "HTTP/1.1";
            this.StatusCode = "200";
            this.ReasonPhrase = "OK";

            this.Headers = new Dictionary<string, string>();
        }

        public void SetStatus(string statusCode, string reasonPhraze)
		{
            // very primitive check but it works
            if (statusCode.Length > 3 || statusCode.Length < 3)
			{
                throw new ArgumentOutOfRangeException();
			}

            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhraze;
        }

        public void SetHeader(string fieldname, string fieldvalue)
		{
            // just to be sure
            if (this.Headers.ContainsKey(fieldname))
			{
                this.Headers[fieldname] = fieldvalue;
			}
            else
			{
                this.Headers.Add(fieldname, fieldvalue);
			}
		}

        public void WriteBody(byte[] body)
		{
            // not much about it
            this.Body = body;
		}

        public byte[] GetBytes()
        {
            // writing status line
            string statusLine = String.Format("{0} {1} {2}\r\n", this.HttpVersion, this.StatusCode, this.ReasonPhrase);
            Console.WriteLine("Status: {0} {1}", this.StatusCode, this.ReasonPhrase);

            // writing headers
            foreach (KeyValuePair<string, string> header in Headers)
            {
                statusLine = String.Format("{0}{1}: {2}\r\n", statusLine, header.Key, header.Value);
            }
            // end header
            statusLine += "\r\n";

            // get header and responce bytes
            byte[] headerBytes = Encoding.UTF8.GetBytes(statusLine);
            byte[] responsBytes = new byte[headerBytes.Length + this.Body.Length];

            // combine data
            Buffer.BlockCopy(headerBytes, 0, responsBytes, 0, headerBytes.Length);
            Buffer.BlockCopy(this.Body, 0, responsBytes, headerBytes.Length, this.Body.Length);

            return responsBytes;
        }
    }
}
