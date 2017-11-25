using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IO
{
	internal static class Program
	{
		//TODO Wnioski do każdego zadania

		private static void Main()
		{
			Zad1();
			Zad2();
			Zad3();
			Zad4();
			Zad5(1024, 64);
			Zad6();
			Zad7();
			Zad8();

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

		private static WaitHandle[] WaitHandle5 { get; set; }

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

			WaitHandle5 = new WaitHandle[num / fragment];

			var ile = 0;
			while (ile < num / fragment)
			{
				WaitHandle5[ile] = new AutoResetEvent(false);
				ThreadPool.QueueUserWorkItem(Zad5CallBack, new object[] {list, fragment, ile, WaitHandle5[ile]});

				lock (Lock)
				{
					ile++;
				}
			}

			WaitHandle.WaitAll(WaitHandle5);
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

		#region Zad6

		private static void Zad6()
		{
			var buffer = new byte[1024];

			var strm = new FileStream(Path.Combine(Environment.CurrentDirectory, "text.txt"), FileMode.Open);

			//var result = strm.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(Zad6CallBack), strm); //To jest to samo co poniżej
			strm.BeginRead(buffer, 0, buffer.Length, Zad6CallBack, strm);
		}

		private static void Zad6CallBack(IAsyncResult result)
		{
			var buffer = new byte[1024];

			var strm = (FileStream) result.AsyncState;

			var numBytes = strm.EndRead(result);

			strm.Close();

			Console.WriteLine($"Read {numBytes} Bytes.");
			//Console.WriteLine(BitConverter.ToString(buffer));
			Console.WriteLine(Encoding.UTF8.GetString(buffer));
		}

		#endregion Zad6

		#region Zad7

		private static void Zad7()
		{
			var buffer = new byte[1024];

			var strm = new FileStream(Path.Combine(Environment.CurrentDirectory, "text.txt"), FileMode.Open);

			var result = strm.BeginRead(buffer, 0, buffer.Length, null, null);

			Thread.Sleep(200);

			//Tutaj są wykonywane rownolegle obliczenia
			//Możemy czytać z 2 osobnych dysków
			//FileStream strm2 = new FileStream(filename2, FileMode.Open);
			//IAsyncResult result2 = strm2.BeginRead(buffer, 0, buffer.Length, null, strm2);

			var numBytes = strm.EndRead(result);

			strm.Close();

			Console.WriteLine($"Read {numBytes} Bytes.");
			//Console.WriteLine(BitConverter.ToString(buffer));
			Console.WriteLine(Encoding.ASCII.GetString(buffer));
		}

		#endregion Zad7

		#region Zad8

		private delegate ulong DelegateType(object arg);

		private static void Zad8()
		{
			const int ktoryWyraz = 20;

			var metoda = new DelegateType[]
			{
				Zad8Silnia_I,
				Zad8Silnia_R,
				Zad8Fib_I,
				Zad8Fib_R
			};

			var result =
			(
				from t
				in metoda
				let m = t
				let time = DateTime.Now
				let wynik = m.EndInvoke(m.BeginInvoke(ktoryWyraz, null, null))
				let span = DateTime.Now - time
				select
				new Tuple<string, ulong, DateTime, TimeSpan>
				(
					t.Method.Name
					, wynik
					, time
					, span
				)
			).ToList();

			/*	Orginał tego powyżej
			var result = new List<Tuple<string, ulong, DateTime, TimeSpan>>();
			foreach (var t in metoda)
			{
				var m = t;

				var time = DateTime.Now;
				var wynik = (ulong) m.EndInvoke(m.BeginInvoke(ktoryWyraz, null, null));
				var span = DateTime.Now - time;

				result.Add(new Tuple<string, ulong, DateTime, TimeSpan>(t.Method.Name, wynik, time, span));
			}
			// */

			var sorted = result.OrderByDescending(r => r.Item2).ToList();

			foreach (var s in sorted)
				Console.WriteLine(
					"Nazwa fukcji wywołanej:\t" + s.Item1 + Environment.NewLine
					+ "Czas rozpoczęcia:\t" + s.Item3 + Environment.NewLine
					+ "Czas wykonania:\t\t" + s.Item4 + Environment.NewLine
					+ "Wynik funkcji:\t\t" + s.Item2 + Environment.NewLine);
		}

		private static ulong Zad8Fib_I(object arg)
		{
			ulong a = 0;
			ulong b = 1;

			var ile = (ulong) arg;

			for (ulong i = 0; i < ile; i++)
			{
				var temp = a;
				a = b;
				b = temp + b;
			}

			return a;
		}

		private static ulong Zad8Fib_R(object arg)
		{
			var n = (ulong) arg;
			if (n < 3)
				return 1;
			return Zad8Fib_R(n - 2) + Zad8Fib_R(n - 1);
		}

		private static ulong Zad8Silnia_I(object arg)
		{
			ulong result = 1;
			for (ulong i = 1; i <= (ulong) arg; i++)
				result *= i;
			return result;
		}

		private static ulong Zad8Silnia_R(object arg)
		{
			var i = (ulong) arg;

			if (i < 1)
				return 1;
			return i * Zad8Silnia_R(i - 1);
		}

		#endregion Zad8
	}
}