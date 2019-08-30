using System;
using System.Collections.Generic;
using System.Text;

namespace Chip8
{
	public class Program
	{
		const string rom = "ibmlogo.ch8";
		public static void Main()
		{
			Console.WriteLine($"Environment.Version: {System.Environment.Version}");
			Console.WriteLine($"RuntimeInformation.FrameworkDescription: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
			Console.WriteLine();

			var state = new State();
			var chip8 = new VirtualMachine(state);

			var size = chip8.Load(rom);
			Console.WriteLine($"Loaded '{rom}' as {size:N0} bytes.");

			var dump = chip8.DumpMemoryString(512..612);
			PrintMemoryDump(dump);

			chip8.EmulateOne();

			var input = Console.ReadKey();
			while (input.KeyChar != 'q')
			{
				chip8.EmulateOne();
				if (input.KeyChar == 'r')
				{
					chip8.Render();
				}

				if (input.KeyChar == 'd')
				{
					chip8.DumpMemory(512..580);
				}


				input = Console.ReadKey();
			}
		}

		public static void PrintMemoryDump(string dump)
		{
			var leftpad = 62;
			var lines = 1;
			var length = 0;

			Console.SetCursorPosition(leftpad, 0);
			Console.WriteLine($"|        00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");

			foreach (var line in dump.Split("\r\n"))
			{
				Console.SetCursorPosition(leftpad, lines);
				Console.WriteLine(line);


				length = Math.Max(length, line.Length);
				lines++;
			}

			Console.SetCursorPosition(leftpad, lines++);
			Console.WriteLine(string.Empty.PadLeft(length, '-'));
		}
	}
}
