﻿using System;

using Chip8.Debugger;
using Chip8.Screens;

namespace Chip8
{
    public class Program
    {
        public static int RomSlot = 7;
        public static bool RunOnStart = true;

        public static string[] Roms = new[]
        {
            @"roms\games\Connect 4 [David Winter].ch8",
            @"roms\games\Space Invaders [David Winter].ch8",
            @"roms\games\Tetris [Fran Dachille, 1991].ch8",
            @"roms\graphics.ch8",
            @"roms\maze.ch8",
            @"roms\paint.ch8",
            @"roms\ibmlogo_top_left.ch8",
            @"roms\bouncy.ch8",
            @"roms\BC_test.ch8",
            @"roms\test_opcode.ch8"
        };

        public static int DisplayScale = 10;

        public static void Main()
        {
            Console.SetWindowSize(Console.WindowWidth, 36);
            Console.WriteLine($"Window={Console.WindowWidth}, {Console.WindowHeight} Buffer={Console.BufferHeight}, {Console.BufferWidth}");
            Console.WriteLine($"Environment.Version: {Environment.Version}");
            Console.WriteLine($"RuntimeInformation.FrameworkDescription: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine();

            var path = Roms[RomSlot];

            var state = new State();
            var screen = new SDLScreen(state, DisplayScale);
            screen.Log = msg => Console.WriteLine(msg);

            var chip8 = new VirtualMachine(state, screen, RunOnStart);
            var size = chip8.Load(path);

            Console.WriteLine($"Loaded '{path}' as {size:N0} bytes.");

            state.RenderMemoryDump(512..612);
            PrintRegisterDump(state.DumpRegisterString());

            var input = Console.ReadKey(true);
            while (input.KeyChar != 'q')
            {
                var skipInstruction = false;
                if (input.KeyChar == 'p')
                {
                    chip8.Pause();
                    skipInstruction = true;
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

                if (!skipInstruction)
                {
                    chip8.ExecuteNext();
                    state.RenderMemoryDump(512..612);
                    PrintRegisterDump(state.DumpRegisterString());
                }

                input = Console.ReadKey(true);
            }

            screen.Dispose();
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
