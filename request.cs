using System;
using System.Collections.Generic;
using System.Text;

namespace HttpWebServer
{
    public class Request
    {
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string Version { get; private set; }

        public Dictionary<string, string> Headers { get; private set; }

        public Request()
        {
            Headers = new Dictionary<string, string>();
        }

        public static Request Parse(string[] Lines)
        {
            Request Parsed = new Request();

            if (Lines.Length < 1)
            {
                return null;
            }

            if (String.IsNullOrEmpty(Lines[0]))
            {
                return null;
            }

            string[] Status = Lines[0].Split(" ");

            if (Status.Length < 3)
            {
                return null;
            }

            Parsed.Method = Status[0];
            Parsed.Path = Status[1];
            Parsed.Version = Status[2];

            Console.WriteLine("Method: {0} Path: {1} Version: {2}", Parsed.Method, Parsed.Path, Parsed.Version);

            for (int i = 1; 1 < Lines.Length; i++)
            {
                string Line = Lines[i];

                if (string.IsNullOrEmpty(Line))
                {
                    break;
                }

                string[] Split = Line.Split(": ");
                Parsed.Headers[Split[0]] = Split[1];
            }

            return Parsed;
        }
    }
}