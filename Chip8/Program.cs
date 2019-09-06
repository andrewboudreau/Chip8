using System;

using Chip8.Debugger;
using Chip8.Screens;

namespace Chip8
{
	public class Program
	{
		public static int Scale = 10;

		public static int RomSlot = 1;

		public static string[] Roms = new[]
		{
			@"roms\maze.ch8",
			@"roms\ibmlogo_top_left.ch8",
			@"roms\bouncy.ch8",
			@"roms\BC_test.ch8",
			@"roms\test_opcode.ch8",
			@"roms\dumps\hires\Hires Test [Tom Swan, 1979].ch8"
		};

		public static void Main()
		{
			Console.SetWindowSize(Console.WindowWidth, 36);
			Console.WriteLine($"Window={Console.WindowWidth}, {Console.WindowHeight} Buffer={Console.BufferHeight}, {Console.BufferWidth}");
			Console.WriteLine($"Environment.Version: {Environment.Version}");
			Console.WriteLine($"RuntimeInformation.FrameworkDescription: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
			Console.WriteLine();

			var path = Roms[RomSlot];

			var state = new State();
			var screen = new SDLScreen(state, 10);
			screen.Log = msg => Console.WriteLine(msg);

			var chip8 = new VirtualMachine(state, screen);

			var size = chip8.Load(path);
			Console.WriteLine($"Loaded '{path}' as {size:N0} bytes.");

			PrintMemoryDump(state.DumpMemoryString(512..612));
			PrintRegisterDump(state.DumpRegisterString());

			var input = Console.ReadKey(true);
			while (input.KeyChar != 'q')
			{
				if (input.KeyChar == 'p')
				{
					chip8.Pause();
				}

				if (input.KeyChar == 'r')
				{
					chip8.Resume();
				}

				if (input.KeyChar == 'c' || Console.CursorTop > 33)
				{
					Console.Clear();
					Console.SetCursorPosition(0, 0);
				}

				if (input.KeyChar == 'f')
				{
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
				}

				if (input.KeyChar == 't')
				{
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
					chip8.ExecuteNext();
				}

				PrintMemoryDump(state.DumpMemoryString(512..612));
				PrintRegisterDump(state.DumpRegisterString());

				input = Console.ReadKey(true);
			}

			screen.Dispose();
		}


		public static void PrintMemoryDump(string dump)
		{
			var left = Console.CursorLeft;
			var top = Console.CursorTop;

			var leftpad = 65;
			var lines = 1;
			var length = 0;

			Console.SetCursorPosition(leftpad, 0);
			Console.WriteLine($"|ADDR 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");

			foreach (var line in dump.Split("\r\n"))
			{
				Console.SetCursorPosition(leftpad, lines);
				Console.WriteLine(line);


				length = Math.Max(length, line.Length);
				lines++;
			}

			Console.SetCursorPosition(leftpad, lines++);
			Console.WriteLine(string.Empty.PadLeft(length, '-'));
			Console.SetCursorPosition(left, top);
		}

		public static void PrintRegisterDump(string dump)
		{
			var left = Console.CursorLeft;
			var top = Console.CursorTop;

			var leftpad = 93;
			var lines = 1;
			var length = 0;
			var hOffset = 8;

			Console.SetCursorPosition(leftpad, hOffset);
			foreach (var line in dump.Split("\r\n"))
			{
				Console.SetCursorPosition(leftpad, hOffset + lines);
				Console.WriteLine(line);

				length = Math.Max(length, line.Length);
				lines++;
			}

			Console.SetCursorPosition(left, top);
		}
	}
}
