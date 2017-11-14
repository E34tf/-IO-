using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IO
{
	internal static class Program
	{
		private static void Main()
		{
			Zad1();
			Zad2();
			Zad3();
			Zad4();
			Zad5(1024, 64);

			Console.ReadKey();
		}

		#region Zad1

		private static void Zad1()
		{
			ThreadPool.QueueUserWorkItem(Zad1CallBack, new object[] {200});
			ThreadPool.QueueUserWorkItem(Zad1CallBack, new object[] {400});
			Thread.Sleep(1000);

			Console.WriteLine("Main thread exits.");
		}

		private static void Zad1CallBack(object contex)
		{
			var t = (int) ((object[]) contex)[0];

			Thread.Sleep(t);

			Console.WriteLine("Zaczekałem: " + t + " [ms]." + Environment.NewLine);
		}

		#endregion Zad1

		#region Zad2

		private static void Zad2()
		{
			ThreadPool.QueueUserWorkItem(Zad2CallBackServer);
			ThreadPool.QueueUserWorkItem(Zad2CallBackClient);
			ThreadPool.QueueUserWorkItem(Zad2CallBackClient);

			Thread.Sleep(5000);
		}

		private static void Zad2CallBackServer(object contex)
		{
			var server = new TcpListener(IPAddress.Any, 1024);
			server.Start();

			while (true)
			{
				var client = server.AcceptTcpClient();

				var buffer = new byte[1024 * 1024];

				client.GetStream().Read(buffer, 0, 1024 * 1024);
				client.GetStream().Write(buffer, 0, buffer.Length);
				client.Close();
			}

			// ReSharper disable once FunctionNeverReturns
		}

		private static void Zad2CallBackClient(object contex)
		{
			var client = new TcpClient();
			client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));
			var message = new ASCIIEncoding().GetBytes("Przykladowa wiadomosc testowa.");
			client.GetStream().Write(message, 0, message.Length);
		}

		#endregion Zad2

		#region Zad3

		private static void Zad3()
		{
			ThreadPool.QueueUserWorkItem(Zad3CallBackServer);
			ThreadPool.QueueUserWorkItem(Zad3CallBackClient);
			ThreadPool.QueueUserWorkItem(Zad3CallBackClient);

			Thread.Sleep(5000);
		}

		private static void Zad3CallBackClient(object contex)
		{
			var client = new TcpClient();
			client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));

			var message = new ASCIIEncoding().GetBytes("Przykladowa wiadomosc testowa.");
			client.GetStream().Write(message, 0, message.Length);

			var msg = "Wyslalem wiadomosc: " + Encoding.ASCII.GetString(message);
			WriteConsoleMessage(msg, ConsoleColor.Green);
		}

		private static void Zad3CallBackServer(object contex)
		{
			var server = new TcpListener(IPAddress.Any, 1024);
			server.Start();

			while (true)
			{
				var client = server.AcceptTcpClient();
				ThreadPool.QueueUserWorkItem(Zad3_ServerCallBack, new object[] {client});
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private static void Zad3_ServerCallBack(object state)
		{
			var client = (TcpClient) ((object[]) state)[0];

			var buffer = new byte[1024];

			client.GetStream().Read(buffer, 0, 1024);
			client.GetStream().Write(buffer, 0, buffer.Length);

			var msg = "Otrzymalem wiadomosc: " + Encoding.ASCII.GetString(buffer);
			client.Close();

			WriteConsoleMessage(msg, ConsoleColor.Red);
		}

		private static void WriteConsoleMessage(string msg, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(msg);
			Console.ResetColor();
		}

		#endregion Zad3

		#region Zad4

		private static object Cc { get; } = new object();

		private static void Zad4()
		{
			ThreadPool.QueueUserWorkItem(Zad4CallBackServer);
			ThreadPool.QueueUserWorkItem(Zad4CallBackClient);
			ThreadPool.QueueUserWorkItem(Zad4CallBackClient);

			Thread.Sleep(5000);
		}

		private static void Zad4CallBackClient(object contex)
		{
			var client = new TcpClient();
			client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));

			var message = new ASCIIEncoding().GetBytes("Przykladowa wiadomosc testowa.");
			client.GetStream().Write(message, 0, message.Length);

			var msg = "Wyslalem wiadomosc: " + Encoding.ASCII.GetString(message);
			WriteConsoleMessage(msg, ConsoleColor.Green, true);
		}

		private static void Zad4CallBackServer(object contex)
		{
			var server = new TcpListener(IPAddress.Any, 1024);
			server.Start();

			while (true)
			{
				var client = server.AcceptTcpClient();
				ThreadPool.QueueUserWorkItem(Zad4_ServerCallBack, new object[] {client});
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private static void Zad4_ServerCallBack(object state)
		{
			var client = (TcpClient) ((object[]) state)[0];

			var buffer = new byte[1024];

			client.GetStream().Read(buffer, 0, 1024);
			client.GetStream().Write(buffer, 0, buffer.Length);

			var msg = "Otrzymalem wiadomosc: " + Encoding.ASCII.GetString(buffer);
			client.Close();

			WriteConsoleMessage(msg, ConsoleColor.Red, true);
		}

		private static void WriteConsoleMessage(string msg, ConsoleColor color, bool czyLock)
		{
			lock (Cc)
			{
				Console.ForegroundColor = color;
				Console.WriteLine(msg);
				Console.ResetColor();
			}

			// ReSharper disable once RedundantAssignment
			czyLock = !czyLock;
		}

		#endregion Zad4

		#region Zad5

		private static object Lock { get; } = new object();

		private static WaitHandle[] WaitHandle { get; set; }

		private static int Suma { get; set; }

		private static void Zad5(int n, int f)
		{
			var temp = n / f;

			if (temp < 64)
				Sum(n, f);
			else
				Console.WriteLine($"Jest więcej watkow niz 64. ({temp} watkow)");
		}

		private static void Sum(int num, int fragment)
		{
			var list = new List<int>();
			for (var i = 0; i < num; i++)
				list.Add(new Random().Next(0, 10));

			WaitHandle = new WaitHandle[num / fragment];

			var ile = 0;
			while (ile < num / fragment)
			{
				WaitHandle[ile] = new AutoResetEvent(false);
				ThreadPool.QueueUserWorkItem(Zad5CallBack, new object[] {list, fragment, ile, WaitHandle[ile]});

				lock (Lock)
				{
					ile++;
				}
			}

			System.Threading.WaitHandle.WaitAll(WaitHandle);
			Console.WriteLine($"Suma jest rowna = {Suma}");
		}

		private static void Zad5CallBack(object state)
		{
			var list = (IList<int>) ((object[]) state)[0];
			var fragment = (int) ((object[]) state)[1];
			var index = (int) ((object[]) state)[2];
			var art = (AutoResetEvent) ((object[]) state)[3];

			for (var i = 0; i < fragment; i++)
				Suma += list[i + index];

			art.Set();
		}

		#endregion Zad5
	}
}