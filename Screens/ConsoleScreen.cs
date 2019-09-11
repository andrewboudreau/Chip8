using System;

namespace Chip8.Screens
{
	public class ConsoleScreen : IScreen
	{
		private readonly State state;

		public ConsoleScreen(State state)
		{
			this.state = state;
		}

		public void Update()
		{
			var rows = state.ScreenBuffer;
			var original = new { Console.CursorLeft, Console.CursorTop };

			var h = 0;
			var left = ((Console.WindowWidth - 66) / 2) - 1;
			var top = (Console.WindowHeight - 34) / 2;
			var totalWidth = 68;

			Console.SetCursorPosition(left, top + h++);
			Console.WriteLine("".PadLeft(totalWidth, '-'));

			for (var y = 0; y < 32; y++)
			{
				var line = Convert.ToString((long)rows[y], 2).PadLeft(64, '0').Replace('1', '█').Replace('0', ' ');
				Console.SetCursorPosition(left, top + h++);
				Console.WriteLine($"{(h - 1).ToString().PadLeft(2)}|{line}|");
			}

			Console.SetCursorPosition(left, top + h++);
			Console.WriteLine("".PadLeft(totalWidth, '-'));

			Console.SetCursorPosition(original.CursorLeft, original.CursorTop);
		}
	}
}
