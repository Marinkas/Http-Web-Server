using System;
using System.Collections.Generic;
using System.Text;

namespace HttpWebServer
{
	public static class Errors
	{
		public static void BadRequest(ref Request request, ref Response response)
		{
			// oops forgot <body> opening tag
			Byte[] d = Encoding.UTF8.GetBytes("<!doctype html><html><head><title>400 Bad Request</title></head><body><h1>Bad Request</h1><p>Your browser sent a request that this server could not understand.</p></body></html>");

			response.Status = "400 Bad Request";
			response.Data = d;

			response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
			response.Headers.Add("Content-Length", d.Length.ToString());
		}

		public static void NotFound(ref Request request, ref Response response)
		{
			Byte[] d = Encoding.UTF8.GetBytes("<!doctype html><html><head><title>404 Not Found</title></head><body><h1>Not Found</h1><p>The requested URL was not found on this server.</p></body></html>");

			response.Status = "404 Not Found";
			response.Data = d;

			response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
			response.Headers.Add("Content-Length", d.Length.ToString());
		}

		public static void InternalServerError(ref Request request, ref Response response)
		{
			Byte[] d = Encoding.UTF8.GetBytes("<!doctype html><html><head><title>500 Internal Server Error</title></head><body><h1>Internal Server Error</h1><p>The server encountered an unexpected condition that prevented it from fulfilling the request.</p></body></html>");

			response.Status = "500 Internal Server Error";
			response.Data = d;

			response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
			response.Headers.Add("Content-Length", d.Length.ToString());
		}
	}
}
