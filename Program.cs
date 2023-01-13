using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace HttpWebServer
{
    class Program
    {
        static HttpServer Server;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Server = new HttpServer(8080, 5);
            Server.Start();
        }
    }
}
