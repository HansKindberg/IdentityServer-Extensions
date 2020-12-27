using System;

namespace Helper
{
	public class ConsoleHelper
	{
		#region Methods

		public static string ReadLine()
		{
			return Console.ReadLine();
		}

		public static void Write(object value)
		{
			Console.Write(value);
		}

		public static void Write(string format, params object[] arguments)
		{
			Console.Write(format, arguments);
		}

		public static void Write(ConsoleColor color, object value)
		{
			Write(color, () => Write(value));
		}

		public static void Write(ConsoleColor color, string format, params object[] arguments)
		{
			Write(color, () => Write(format, arguments));
		}

		private static void Write(ConsoleColor color, Action writeAction)
		{
			Console.ForegroundColor = color;
			writeAction();
			Console.ResetColor();
		}

		public static void WriteBlue(object value)
		{
			Write(ConsoleColor.Blue, value);
		}

		public static void WriteBlue(string format, params object[] arguments)
		{
			Write(ConsoleColor.Blue, format, arguments);
		}

		public static void WriteBlueLine(object value)
		{
			WriteLine(ConsoleColor.Blue, value);
		}

		public static void WriteBlueLine(string format, params object[] arguments)
		{
			WriteLine(ConsoleColor.Blue, format, arguments);
		}

		public static void WriteGreen(object value)
		{
			Write(ConsoleColor.Green, value);
		}

		public static void WriteGreen(string format, params object[] arguments)
		{
			Write(ConsoleColor.Green, format, arguments);
		}

		public static void WriteGreenLine(object value)
		{
			WriteLine(ConsoleColor.Green, value);
		}

		public static void WriteGreenLine(string format, params object[] arguments)
		{
			WriteLine(ConsoleColor.Green, format, arguments);
		}

		public static void WriteLine(object value)
		{
			Console.WriteLine(value);
		}

		public static void WriteLine(string format, params object[] arguments)
		{
			Console.WriteLine(format, arguments);
		}

		public static void WriteLine(ConsoleColor color, object value)
		{
			Write(color, () => WriteLine(value));
		}

		public static void WriteLine(ConsoleColor color, string format, params object[] arguments)
		{
			Write(color, () => WriteLine(format, arguments));
		}

		public static void WriteRed(object value)
		{
			Write(ConsoleColor.Red, value);
		}

		public static void WriteRed(string format, params object[] arguments)
		{
			Write(ConsoleColor.Red, format, arguments);
		}

		public static void WriteRedLine(object value)
		{
			WriteLine(ConsoleColor.Red, value);
		}

		public static void WriteRedLine(string format, params object[] arguments)
		{
			WriteLine(ConsoleColor.Red, format, arguments);
		}

		public static void WriteYellow(object value)
		{
			Write(ConsoleColor.Yellow, value);
		}

		public static void WriteYellow(string format, params object[] arguments)
		{
			Write(ConsoleColor.Yellow, format, arguments);
		}

		public static void WriteYellowLine(object value)
		{
			WriteLine(ConsoleColor.Yellow, value);
		}

		public static void WriteYellowLine(string format, params object[] arguments)
		{
			WriteLine(ConsoleColor.Yellow, format, arguments);
		}

		#endregion
	}
}