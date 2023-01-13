using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace HttpWebServer
{
	public class Request
	{
		public string Method { get; private set; }
		public string Path { get; private set; }
		public string Version { get; private set; }

		public Dictionary<string, string> Headers { get; private set; }

		public char[] Body { get; private set; }

		// Extra
		public string Host { get; private set; }
		public string[] Encodings { get; private set; }
		public Dictionary<string, string> Cookies { get; private set; }
		public Dictionary<string, string> Query { get; private set; }

		public bool IsBad { get; private set; }

		public Request()
		{
			Headers = new Dictionary<string, string>();
			Cookies = new Dictionary<string, string>();
		}

		public void ParsePath()
		{
			string[] path_query = Path.Split("?");
			Path = path_query[0].Replace('/', '\\');

			if (path_query.Length > 1)
			{
				string query = path_query[1];
				Query = Request.ParseQuery(query);
			}
		}

		public void ParseHeaders()
		{
			foreach (KeyValuePair<string, string> Header in Headers)
			{
				switch (Header.Key)
				{
					case "Host":
						Host = Header.Value;
						break;
					case "Accept-Encoding":
						string[] _Encodings = Header.Value.Split(", ");

						for (int i = 0; i < _Encodings.Length; i++)
						{
							string Encoding = _Encodings[i];
							int QStart = Encoding.IndexOf(';');

							if (QStart > 0)
							{
								string QValStr = Encoding.Substring(QStart + 1, Encoding.Length - QStart - 1);
								float QVal = float.Parse(QValStr.Substring(2));
								Console.WriteLine(QVal);
							}
						}
						break;
					case "Cookie":
						if (String.IsNullOrEmpty(Header.Value))
						{
							break;
						}

						string[] _Cookies = Header.Value.Split("; ");

						for (int i = 0; i < _Cookies.Length; i++)
						{
							string[] Cookie = _Cookies[i].Split("=");
							Cookies.Add(Cookie[0], Cookie[1]);
						}

						break;
					default:
						break;
				}
			}
		}

		public static Dictionary<string, string> ParseQuery(string query)
		{
			Dictionary<string, string> Query = new Dictionary<string, string>();

			string[] vars = query.Split('&');

			for (int i = 0; i < vars.Length; i++)
			{
				string[] pair = vars[i].Split('=');

				string field = HttpUtility.UrlDecode(pair[0]);
				string value = HttpUtility.UrlDecode(pair[1]);

				Query.Add(field, value);
			}

			return Query;
		}

		public static Request Parse(NetworkStream stream)
		{
			try
			{
				StreamReader reader = new StreamReader(stream);

				Request Parsed = new Request();

				string requestLine = reader.ReadLine();

				if (string.IsNullOrEmpty(requestLine))
				{
					return null;
				}

				if (requestLine.Length > 8000)
				{
					return null;
				}

				string[] Status = requestLine.Split(" ");

				if (Status.Length < 3)
				{
					return null;
				}

				Parsed.Method = Status[0];
				Parsed.Path = Status[1];
				Parsed.Version = Status[2];

				if (Parsed.Method.ToUpper() != "GET" && Parsed.Method.ToUpper() != "POST")
				{
					return null;
				}

				while (reader.Peek() > -1)
				{
					string HeaderStr = reader.ReadLine();

					if (string.IsNullOrEmpty(HeaderStr))
					{
						break;
					}

					string[] Header = HeaderStr.Split(": ");
					Parsed.Headers[Header[0]] = Header[1];
				}

				if (Parsed.Method == "POST")
				{
					if (Parsed.Headers.ContainsKey("Content-Length"))
					{
						string HeaderVal = Parsed.Headers["Content-Length"];
						int length = int.Parse(HeaderVal);

						Parsed.Body = new char[length];
						reader.ReadBlock(Parsed.Body, 0, length);

						Console.WriteLine("body: " + (new string(Parsed.Body)));
					}
				}

				Parsed.ParsePath();
				Parsed.ParseHeaders();

				Console.WriteLine("Method: {0} Path: {1} Version: {2}", Parsed.Method, Parsed.Path, Parsed.Version);

				return Parsed;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
	}
}