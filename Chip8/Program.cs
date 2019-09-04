using System;

namespace Chip8
{
	public class Program
	{
		public static int RomSlot = 1;

		public static string[] Roms = new[]
		{
			@"roms\ibmlogo.ch8",
			@"roms\ibmlogo_top_left.ch8",
			@"roms\bouncy.ch8",
			@"roms\BC_test.ch8",
			@"roms\test_opcode.ch8"
		};

		public static void Main()
		{
			Console.SetWindowSize(Console.WindowWidth, 36);
			Console.WriteLine($"Window={Console.WindowWidth}, {Console.WindowHeight} Buffer={Console.BufferHeight}, {Console.BufferWidth}");
			Console.WriteLine($"Environment.Version: {System.Environment.Version}");
			Console.WriteLine($"RuntimeInformation.FrameworkDescription: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
			Console.WriteLine();

			var state = new State();
			var chip8 = new VirtualMachine(state);

			var path = Roms[RomSlot];
			var size = chip8.Load(path);
			Console.WriteLine($"Loaded '{path}' as {size:N0} bytes.");

			chip8.EmulateOne();
			chip8.EmulateOne();
			chip8.EmulateOne();
			chip8.EmulateOne();
			chip8.EmulateOne();

			PrintMemoryDump(chip8.DumpMemoryString(512..612));
			PrintRegisterDump(chip8.DumpRegisterString());

			var input = Console.ReadKey(true);
			while (input.KeyChar != 'q')
			{
				chip8.EmulateOne();

				PrintMemoryDump(chip8.DumpMemoryString(512..612));
				PrintRegisterDump(chip8.DumpRegisterString());

				if (input.KeyChar == 'r')
				{
					PrintScreen(chip8.Render());
				}

				if (input.KeyChar == 'd')
				{
					chip8.DumpMemory(512..580);
				}

				if (input.KeyChar == 'c')
				{
					Console.Clear();
					Console.SetCursorPosition(0, 0);
				}

				if (input.KeyChar == 'f')
				{
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
				}

				if (input.KeyChar == 't')
				{
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
					chip8.EmulateOne();
				}

				input = Console.ReadKey(true);
			}
		}

		public static void PrintScreen(string dump)
		{
			var original = new { Console.CursorLeft, Console.CursorTop };

			var h = 0;
			var left = ((Console.WindowWidth - 66) / 2) - 1;
			var top = (Console.WindowHeight - 34) / 2;
			var totalWidth = 68;

			Console.SetCursorPosition(left, top + h++);
			Console.WriteLine("".PadLeft(totalWidth, '-'));

			foreach (var line in dump.Split("\r\n"))
			{
				Console.SetCursorPosition(left, top + h++);
				Console.WriteLine($"{(h - 1).ToString().PadLeft(2)}|{line}|");
			}

			Console.SetCursorPosition(left, top + h++);
			Console.WriteLine("".PadLeft(totalWidth, '-'));

			Console.SetCursorPosition(original.CursorLeft, original.CursorTop);
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
