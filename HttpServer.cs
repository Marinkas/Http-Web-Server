using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HttpWebServer
{
	public class HttpServer
	{
		private int Port;
		private int Backlog;

		private TcpListener Listener;
		private Thread CommThread;
		private FileRouter StaticRouter;

		private bool Running = false;

		public HttpServer(int Port = 80, int Backlog = 5)
		{
			this.Port = Port;
			this.Backlog = Backlog;

			this.Listener = new TcpListener(IPAddress.Any, this.Port);
			this.CommThread = new Thread(new ThreadStart(this.Listening));

			this.StaticRouter = new FileRouter(Environment.CurrentDirectory + "\\static");
		}

		public void Start()
		{
			Listener.Start(this.Backlog);

			Running = true;

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

			string date = String.Format("{0:ddd,' 'dd' 'MMM' 'yyyy' 'HH':'mm':'ss' GMT'}", DateTime.UtcNow);

			response.SetHeader("Date", date);
			response.SetHeader("Server", "test server");

			byte[] returnDat = response.GetBytes();

			stream.Write(returnDat, 0, returnDat.Length);
			stream.Flush();

			client.Close();
		}

		private void HandleReq(ref Request request, ref Response response)
		{
			try
			{
				if (request == null)
				{
					Errors.BadRequest(ref request, ref response);
				}
				else
				{
					// send to file router
					this.StaticRouter.HandleReq(ref request, ref response);
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Errors.InternalServerError(ref request, ref response);
			}
		}
	}
}
