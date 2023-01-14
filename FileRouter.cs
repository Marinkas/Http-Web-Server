using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpWebServer
{
	public class FileRouter
	{
		private string root;

		private Dictionary<string, string> mimeTypes;
		private string[] defaultFiles;

		public FileRouter(string root)
		{
			this.root = root;

			this.loadMimeTypes();
			this.loadDefaultFiles();
		}

		public void HandleReq(ref Request request, ref Response response)
		{
			// file or folder path
			string path = this.root + request.Path.Trim();

			if (!VerifyPathUnderRoot(path, this.root))
			{
				Errors.BadRequest(ref request, ref response);
				return;
			}

			// if its a directory
			if (Directory.Exists(path))
			{
				// send the directory's index file or 404
				bool foundDefault = false;

				if (!path.EndsWith('\\'))
				{
					path += '\\';
				}

				for (int i = 0; i < this.defaultFiles.Length; i++)
				{
					string defaultFile = this.defaultFiles[i];
					string filepath = path + defaultFile;

					if (File.Exists(filepath))
					{
						// found default file
						foundDefault = true;
						path = filepath;

						break;
					}
				}

				if (foundDefault)
				{
					// found index
					SendFile(path, ref request, ref response);
				}
				else
				{
					// not found
					Errors.NotFound(ref request, ref response);
				}
				return;
			}

			// if its a file
			if (File.Exists(path))
			{
				// return the file
				SendFile(path, ref request, ref response);
				return;
			}
			// special case for html files make this apply to all file formats later
			else if (File.Exists(path + ".html"))
			{
				SendFile(path + ".html", ref request, ref response);
				return;
			}

			// not found
			Errors.NotFound(ref request, ref response);
			return;
		}

		private void SendFile(string filepath, ref Request request, ref Response response)
		{
			FileInfo f = new FileInfo(filepath);

			if (!f.Exists)
			{
				Errors.NotFound(ref request, ref response);
				return;
			}

			string t = this.GetMimeType(f.Extension);

			Byte[] d = FileBytes(f);
			//d = Compress(d); // later versions sould only apply diffrent compression algorythms depending on the file format

			response.SetStatus("200", "OK");

			response.WriteBody(d);

			//response.SetHeader("Content-Encoding", "gzip");
			response.SetHeader("Content-Length", d.Length.ToString());
			response.SetHeader("Content-Type", String.Format("{0}; charset=UTF-8", t));
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

		public static byte[] Compress(byte[] data)
		{
			using (var compressedStream = new MemoryStream())
			using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
			{
				zipStream.Write(data, 0, data.Length);
				zipStream.Close();
				return compressedStream.ToArray();
			}
		}

		private static bool VerifyPathUnderRoot(string pathToVerify, string rootPath = ".")
		{
			var fullRoot = Path.GetFullPath(rootPath);
			var fullPathToVerify = Path.GetFullPath(pathToVerify);
			return fullPathToVerify.StartsWith(fullRoot);
		}

		public string GetMimeType(string Extension)
		{
			if (Extension == null)
				throw new ArgumentNullException("extension");

			if (Extension.StartsWith("."))
				Extension = Extension.Substring(1);

			if (this.mimeTypes.ContainsKey(Extension))
				return this.mimeTypes[Extension];

			return "application/octet-stream";
		}

		private void loadMimeTypes()
		{
			string path = Environment.CurrentDirectory + "\\mime.dat";

			string[] lines = File.ReadAllLines(path);
			this.mimeTypes = new Dictionary<string, string>();

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				
				// null, empty or whitespace check
				if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				string[] args = line.Split(";");

				// sanity check
				if (args.Length < 2)
				{
					continue;
				}

				string extension = args[0].Trim();
				string mimeType = args[1].Trim();

				// more sanity checks
				if (string.IsNullOrEmpty(extension) || string.IsNullOrWhiteSpace(extension))
				{
					continue;
				}

				if (string.IsNullOrEmpty(mimeType) || string.IsNullOrWhiteSpace(mimeType))
				{
					continue;
				}

				if (this.mimeTypes.ContainsKey(extension))
				{
					this.mimeTypes[extension] = mimeType;
				}
				else
				{
					this.mimeTypes.Add(extension, mimeType);
				}
			}
		}

		private void loadDefaultFiles()
		{
			string path = Environment.CurrentDirectory + "\\default.dat";

			string[] lines = File.ReadAllLines(path);
			List<string> parsedLines = new List<string>();

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];

				// null line check
				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				if (Regex.IsMatch(line, @"^[A-Za-z0-9]+\.[A-Za-z0-9]+$")) // basic check to filter out invalid lines
				{
					parsedLines.Add(line);
				}
			}

			this.defaultFiles = parsedLines.ToArray();
		}
	}
}
