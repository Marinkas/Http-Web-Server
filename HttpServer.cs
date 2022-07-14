using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

namespace HttpWebServer
{
	public class HttpServer
	{
		private int Port = 8080;
		private int Backlog = 5;

		private TcpListener Listener;
		private Thread CommThread;

		private bool Running = false;

		public HttpServer()
		{
			Port = 8080;
			Backlog = 5;

			Listener = new TcpListener(IPAddress.Any, Port);
			Listener.Start(Backlog);

			Running = true;

			CommThread = new Thread(new ThreadStart(Listening));
			CommThread.Start();
		}

		private void Listening()
		{
			while (Running)
			{
				// Accept incoming connections
				if (Listener.Pending())
				{
					TcpClient client = Listener.AcceptTcpClient();
					ThreadPool.QueueUserWorkItem(HandleClient, client);
				}

				// Reduce CPU usage
				Thread.Sleep(50);
			}

			Listener.Stop();
		}

		private void HandleClient(object state)
		{
			TcpClient client = (TcpClient)state;

			NetworkStream stream = client.GetStream();

			IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
			
			Console.WriteLine("[Request] From: {0}", endPoint.Address.ToString());

			Request request = Request.Parse(stream);
			Response response = new Response();

			HandleReq(ref request, ref response);

			string date = String.Format("{0:ddd,' 'dd' 'MMM' 'yyyy' 'HH':'mm':'ss' 'K}", DateTime.Now, CultureInfo.InvariantCulture);
			
			response.Headers.Add("Date", date);
			response.Headers.Add("Server", "test server");

			byte[] returnDat = response.GetBytes();

			stream.Write(returnDat, 0, returnDat.Length);
			stream.Flush();

			client.Close();
		}

		private void HandleReq(ref Request request, ref Response response)
		{
			if (request == null)
			{
				response.Status = "400 Bad Request";
				response.Data = new byte[0];

				//response.Headers.Add("Content-Type", "application/octet-stream; charset=UTF-8");
				//response.Headers.Add("Content-Length", "0");
			}
			else
			{
				foreach (KeyValuePair<string, string> Cookie in request.Cookies)
				{
					Console.WriteLine($"Cookie Name: {Cookie.Key} | Value: {Cookie.Value}");
				}

				if (request.Path == "/")
				{
					String file = Environment.CurrentDirectory + "/www/index.html";
					FileInfo f = new FileInfo(file);

					Byte[] d = FileBytes(f);

					response.Status = "200 OK";
					response.Data = d;

					response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
					response.Headers.Add("Content-Length", d.Length.ToString());
				}
				else
				{
					String file = Environment.CurrentDirectory + "/www" + request.Path;
					FileInfo f = new FileInfo(file);
					if (f.Exists & f.Extension.Contains("."))
					{
						string t = Program.GetMimeType(f.Extension);
						Byte[] d = FileBytes(f);

						response.Status = "200 OK";
						response.Data = d;

						response.Headers.Add("Content-Type", String.Format("{0}; charset=UTF-8", t));
						response.Headers.Add("Content-Length", d.Length.ToString());
					}
					else
					{
						response.Status = "404 Not Found";
						response.Data = new byte[0];

						//response.Headers.Add("Content-Type", "application/octet-stream; charset=UTF-8");
						//response.Headers.Add("Content-Length", "0");
					}
				}
			}
		}

		private byte[] FileBytes(FileInfo fi)
		{
			FileStream fs = fi.OpenRead();
			BinaryReader reader = new BinaryReader(fs);
			Byte[] d = new Byte[fs.Length];
			reader.Read(d, 0, d.Length);
			fs.Close();

			return d;
		}
	}
}
