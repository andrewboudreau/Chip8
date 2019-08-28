using System;
using System.Collections;
using System.IO;

namespace Chip8
{

    public class VirtualMachinePrototype
    {
        private readonly Memory<byte> memory;

        private static Action<string> log = Console.Write;
        private static Action<string> logline = Console.WriteLine;

        public VirtualMachinePrototype(Memory<byte> memory)
        {
            this.memory = memory;
        }

        internal void Run(string file)
        {
            var sizeInBytes = LoadHex(file);
            logline($"Loaded '{file}' as {sizeInBytes:N0} bytes.");
        }

        private int LoadHex(string file)
        {
            return new FileStream(file, FileMode.Open).Read(memory.Span[512..]);
        }

        /// Instruction Pointer
        public ushort ip = 512;
        public ushort i = 0;

        internal void DumpMemory(Range range)
        {
            logline($"Dumping Memory {range.Start.Value}(0x{range.Start.Value:X4}) to {range.End.Value}(0x{range.End.Value:X4})");

            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                log($"{memory.Span[i]:x2} ");

                if ((i + 1) % 16 == 0)
                {
                    logline(string.Empty);
                }
            }
        }

        /// <summary>
        /// Writes a string at the x position, y position = 1;
        /// Tries to catch all exceptions, will not throw any exceptions.  
        /// </summary>
        /// <param name="s">String to print usually "*" or "@"</param>
        /// <param name="x">The x postion,  This is modulo divided by the window.width, 
        /// which allows large numbers, ie feel free to call with large loop counters</param>
        public static void WriteAt(string s, int x, int y)
        {
            int origRow = Console.CursorTop;
            int origCol = Console.CursorLeft;
            // Console.WindowWidth = 10;  // this works. 

            try
            {
                Console.SetCursorPosition(x, y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
            }
            finally
            {
                try
                {
                    Console.SetCursorPosition(origRow, origCol);
                }
                catch (ArgumentOutOfRangeException e)
                {
                }
            }
        }

        /// <summary>
        /// 16 8-bit registers (128 bits), V0-VF.
        /// 1 16-bit register (16 bits), I
        /// </summary>
        Memory<byte> Registers { get; }

        object Stack { get; }

        object Timers { get; }

        object Input { get; }
    }

    public struct Register
    {
        byte Byte;
    }

    public static class Display2
    {
        // chip-8 display has 64x32 pixels 2048 (0x0800) bits, (256 Bytes)
        public readonly static BitArray Screen = new BitArray(new byte[256]);

        public static void Clear()
        {
            Screen.SetAll(false);
        }

        public static void Fill()
        {
            Screen.SetAll(true);
        }

        public static void Render()
        {
            Console.WriteLine(string.Empty.PadLeft(64, '-'));
            var i = 0;
            Console.Write("0 - ".PadLeft(9, ' '));
            foreach (var bit in Screen)
            {
                Console.Write((bool)bit ? "█" : " ");
                if ((++i) % 64 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i} - ".PadLeft(9, ' '));
                }
            }

            Console.WriteLine("".PadLeft(64, '-'));
        }
    }

    public static class Instructions
    {
        public static void Invoke(ushort word)
        {
            var first = (byte)(word >> 8);
            var second = (byte)(word & 255);

            Console.WriteLine($"0x{first:X2}{second:x2}");
            Console.WriteLine($"0x{(word & 0x0FFF):x4}");
        }

        public static void Invoke(byte first, byte second, VirtualMachinePrototype chip8)
        {
            var operation = first & 0xF0;
            var register = first & 0x0F;
            var left = second & 0xF0;
            var right = second & 0x0F;
            ushort n = (ushort)(((first << 0x8) + second) & 0x0FFF);

            Console.WriteLine($"0x{first:X2}{second:x2}");
            Console.WriteLine($"  First=0x{first:x2} Op=0x{operation:x1}, Reg=0x{register:x1}");
            Console.WriteLine($"  Second=0x{second:X2} (0x{left:X1}, 0x{right:X1})");
            Console.WriteLine($"  n={n:x4}");

            switch (operation)
            {
                case 0x00:
                    if (second == 0xE0)
                    {
                        break;
                    }
                    else if (second == 0xEE)
                    {
                        Console.WriteLine("Return from subroutine.");
                        break;
                    }

                    throw new InvalidOperationException($"operation=0x{operation:X2} register=0x{register:X2} args=0x{second:x2}");

                case 0xA0:
                    chip8.i = n;
                    Console.WriteLine($"Set Regiser I to 0x{n:x3}");
                    break;

                case 0x60:
                    Console.WriteLine($"Sets Register V{register:X1} to 0x{second:X2}.");
                    break;

                case 0xD0:
                    Console.WriteLine($"Draw (Vx=0x{register:X1}, Vy=0x{left:X1}, N={right:x1} at Memory={chip8.i:x3}");
                    break;

                case 0x70:
                    Console.WriteLine($"Adds {right}(0x{right:x2}) to V{register:x1}. (Carry flag is not changed)");
                    break;

                default:
                    throw new InvalidOperationException($"operation=0x{operation:X2} register=0x{register:X2} args=0x{second:x2}");
            }
        }
    }

    public struct RegistersOG
    {
        byte[] Data { get; }
    }
}
