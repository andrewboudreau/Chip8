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

            chip8.DumpMemory(512..580);
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
    }
}
