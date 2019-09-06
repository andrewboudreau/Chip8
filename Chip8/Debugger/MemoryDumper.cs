using System;
using System.Text;

namespace Chip8.Debugger
{
    public static class MemoryDumper
    {
        internal static string DumpMemoryString(this State state, Range range)
        {
            var buffer = new StringBuilder();
            var line = 0;



            if ((range.End.Value - range.Start.Value) % 16 != 0)
            {
                //buffer.AppendLine();
            }

            return buffer.ToString();
        }

        internal static void RenderMemoryDump(this State state, Range range)
        {
            var left = Console.CursorLeft;
            var top = Console.CursorTop;

            var leftpad = 65;
            var lines = 1;
            var datarow = 0;

            Console.SetCursorPosition(leftpad, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"│ADDR 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            Console.SetCursorPosition(leftpad, lines++);
            Console.Write($"│{range.Start.Value:X4} ");
            Console.ResetColor();

            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                if (i == state.ProgramCounter || i == state.ProgramCounter + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (i == state.Index)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }

                Console.Write($"{state.Memory[i]:X2} ");
                Console.ResetColor();

                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine();
                    Console.SetCursorPosition(leftpad, lines++);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"│{range.Start.Value + (++datarow * 16):X4} ");
                    Console.ResetColor();
                }
            }

            Console.SetCursorPosition(leftpad, lines++);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("└───────────────────────────┬──────────────────────────");
            //Console.WriteLine("└".PadRight(Console.BufferWidth - leftpad, '─'));
            Console.ResetColor();
            Console.SetCursorPosition(left, top);
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

        internal static string DumpRegisterString(this State state)
        {
            var buffer = new StringBuilder();

            buffer.Append($"│ Index = #{state.Index:X3} ");
            buffer.AppendLine($"IP = #{state.ProgramCounter:X3} ");

            for (var i = 0; i < 16; i++)
            {
                buffer.AppendLine($"│ V{i:X1} = #{state.Registers[i]:X2}");
            }

            return buffer.ToString();
        }
    }
}
