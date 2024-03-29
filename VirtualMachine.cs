﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Chip8
{
    public class VirtualMachine
    {
        private readonly Random rand = new Random(0);

        private readonly State state;
        private readonly IScreen screen;
        private readonly Clock clock;

        public VirtualMachine(State state, IScreen screen, bool startRunning = true)
        {
            this.state = state;
            this.screen = screen;

            LoadFonts(this.state);
            clock = new Clock(Tick60Hz, ExecuteNext, startRunning);
        }

        public void ExecuteNext()
        {
            var high = state.Memory[state.ProgramCounter];
            var low = state.Memory[state.ProgramCounter + 1];

            Execute(new Instruction(high, low));
        }

        public void Execute(Instruction instruction)
        {
            switch (instruction.Code)
            {
                case 0x0:
                    switch (instruction.NN)
                    {
                        case 0xE0:
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tCLS");
                            state.ScreenBuffer.Fill(0x0);
                            state.ProgramCounter += 2;

                            screen.Update();
                            break;

                        case 0xEE:
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tRTS");
                            if (state.StackPointer == 0)
                            {
                                throw new Exception(" Program Halted.");
                            }

                            state.ProgramCounter = state.Stack[--state.StackPointer];
                            state.ProgramCounter += 2;

                            break;

                        default:
                            throw new InvalidOperationException($"{instruction.Value:X4}, PC=#{state.ProgramCounter:X4} NOT AN INSTRUCTION");
                    }

                    break;
                case 0x1:
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tJMP\t#{instruction.NNN:X3}");
                    state.ProgramCounter = instruction.NNN;
                    break;

                case 0x2:
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tCALL\t#{instruction.NNN:X3}");
                    state.Stack[state.StackPointer++] = state.ProgramCounter;
                    state.ProgramCounter = instruction.NNN;
                    break;

                case 0x3:
                    // Skips the next instruction if VX equals NN.
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSE\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
                    if (state.Registers[instruction.X] == instruction.NN)
                    {
                        state.ProgramCounter += 2;
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0x4:
                    // Skips the next instruction if VX doesn't equal NN.
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSNE\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
                    if (state.Registers[instruction.X] != instruction.NN)
                    {
                        state.ProgramCounter += 2;
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0x5:
                    // Skips the next instruction if VX equals VY.
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSE\tV{instruction.X:X1}({state.Registers[instruction.X]}), V{instruction.Y:X1}({state.Registers[instruction.Y]})");
                    if (state.Registers[instruction.X] == state.Registers[instruction.Y])
                    {
                        state.ProgramCounter += 2;
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0x6:
                    // Load NN into RX
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, #{instruction.NN:X2}");
                    state.Registers[instruction.X] = instruction.NN;
                    state.ProgramCounter += 2;
                    break;

                case 0x7:
                    // Adds NN to VX. (Carry flag is not changed)
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tADD\tV{instruction.X:X1}, #{instruction.NN:X2}");
                    state.Registers[instruction.X] += instruction.NN;
                    state.ProgramCounter += 2;
                    break;

                case 0x8:
                    switch (instruction.N)
                    {
                        case 0x0:
                            //Stores the value of register Vy in register Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[instruction.X] = state.Registers[instruction.Y];
                            break;

                        case 0x1:
                            // Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tOR\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[instruction.X] |= state.Registers[instruction.Y];
                            break;

                        case 0x2:
                            // Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tAND\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[instruction.X] &= state.Registers[instruction.Y];
                            break;

                        case 0x3:
                            // Performs a bitwise XOR on the values of Vx and Vy, then stores the result in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tAND\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[instruction.X] ^= state.Registers[instruction.Y];
                            break;

                        case 0x4:
                            // The values of Vx and Vy are added together. 
                            // If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. 
                            // Only the lowest 8 bits of the result are kept, and stored in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tADD\tV{instruction.X:X1}, V{instruction.Y:X2}(VF={state.Registers[0xF]})");
                            state.Registers[0xF] = (byte)(state.Registers[instruction.X] + state.Registers[instruction.Y] > 0xFFFF ? 1 : 0);
                            state.Registers[instruction.X] += state.Registers[instruction.Y];
                            break;

                        case 0x5:
                            // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            // If Vx > Vy, then VF is set to 1, otherwise 0.
                            // Then Vy is subtracted from Vx, and the results stored in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSUB\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[0xF] = (byte)(state.Registers[instruction.X] > state.Registers[instruction.Y] ? 1 : 0);
                            state.Registers[instruction.X] -= state.Registers[instruction.Y];
                            break;

                        case 0x6:
                            // Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSHR\tV{instruction.X:X1}");
                            state.Registers[0xF] = (byte)(state.Registers[instruction.X] & 0x01);
                            state.Registers[instruction.X] = (byte)(state.Registers[instruction.X] >> 1);
                            break;

                        case 0x7:
                            // If Vy > Vx, then VF is set to 1, otherwise 0.
                            // Then Vx is subtracted from Vy, and the results stored in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSUBN\tV{instruction.X:X1}, V{instruction.Y:X2}");
                            state.Registers[0xF] = (byte)(state.Registers[instruction.Y] > state.Registers[instruction.X] ? 1 : 0);
                            state.Registers[instruction.X] -= state.Registers[instruction.Y];
                            break;

                        case 0xE:
                            // If the most - significant bit of Vx is 1, then VF is set to 1, otherwise to 0.
                            // Then Vx is multiplied by 2.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSHL\tV{instruction.X:X1}");
                            state.Registers[0xF] = (byte)((state.Registers[instruction.X] & (1 << 7)) >> 7);
                            state.Registers[instruction.X] = (byte)(state.Registers[instruction.X] << 1);
                            break;

                        default:
                            throw new InvalidOperationException($"{instruction.Value:x4} NOT AN INSTRUCTION");
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0x9:
                    // Skips the next instruction if VX doesn't equal VY.  
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSNE\tV{instruction.X:X1}({state.Registers[instruction.X]}), V{instruction.Y:X1}({state.Registers[instruction.Y]})");
                    if (state.Registers[instruction.X] != state.Registers[instruction.Y])
                    {
                        state.ProgramCounter += 2;
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0xA:
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tI, #{instruction.NNN:X3}");
                    state.Index = instruction.NNN;
                    state.ProgramCounter += 2;
                    break;

                case 0xB:
                    // Jumps to the address NNN plus V0.
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tJMI\t#{instruction.NNN:X3}, V0({state.Registers[0]}");
                    state.ProgramCounter = (ushort)(instruction.NNN + state.Registers[0]);
                    break;

                case 0xC:
                    // register VX = random number AND KK
                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tRND\tV{instruction.X:X1}, #{instruction.NN:x2}");
                    state.Registers[instruction.X] = (byte)(rand.Next(0, 256) & instruction.NN);
                    state.ProgramCounter += 2;
                    break;

                case 0xD:
                    // Draw sprite for memory location to screen memory at X,Y screen coordinates.
                    // Sprites are XOR'ed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. 
                    // If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
                   
                    var x = state.Registers[instruction.X] % 64;
                    var y = state.Registers[instruction.Y] % 32;
                    var anyPixlesErased = false;

                    Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tDRW\tV{instruction.X:X1}({x}), V{instruction.Y:X1}({y}), {instruction.N}");
                    for (var n = 0; n < instruction.N; n++)
                    {
                        var horz = (y + n) % 32;
                        var sprite = (ulong)state.Memory[state.Index + n] << (64 - 8 - x);

                        anyPixlesErased |= (sprite & state.ScreenBuffer[horz]) > 0;

                        // Handle any pixesl that wrapped the screen.
                        if (x > (64 - 8))
                        {
                            var wrappedSprite = sprite >> (x - (64 - 8));
                            anyPixlesErased |= (wrappedSprite & state.ScreenBuffer[horz]) > 0;
                            state.ScreenBuffer[horz] ^= wrappedSprite;
                        }

                        // xor data from memory into screen memory
                        state.ScreenBuffer[horz] ^= sprite;
                    }

                    // Set VF to 1 if any pixels are erased.
                    state.Registers[0xF] = (byte)(anyPixlesErased ? 1 : 0);
                    state.ProgramCounter += 2;
                    screen.Update();
                    break;

                case 0xE:
                    switch (instruction.NN)
                    {
                        case 0x9E:
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSKP\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
                            // Skip next instruction if key with the value of Vx is pressed.
                            // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
                            if ((state.Keys & (1 << state.Registers[instruction.X])) > 0)
                            {
                                state.ProgramCounter += 2;
                            }

                            break;

                        case 0xA1:
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tSKNP\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
                            // Skip next instruction if key with the value of Vx is not pressed.
                            // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
                            if ((state.Keys & (1 << state.Registers[instruction.X])) == 0)
                            {
                                state.ProgramCounter += 2;
                            }

                            break;

                        default:
                            throw new InvalidOperationException($"{instruction.Value:X4} NOT AN INSTRUCTION");
                    }

                    state.ProgramCounter += 2;
                    break;

                case 0xF:
                    switch (instruction.NN)
                    {
                        case 0x07:
                            // Sets VX to the value of the delay timer.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, DELAY");
                            state.Registers[instruction.X] = state.DelayTimer;
                            state.ProgramCounter += 2;
                            break;

                        case 0x15:
                            // Sets the delay timer to VX.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tDELAY, V{instruction.X:X1}");
                            state.DelayTimer = state.Registers[instruction.X];
                            state.ProgramCounter += 2;
                            break;

                        case 0x0A:
                            // Wait for a key press, store the value of the key in Vx.
                            // All execution stops until a key is pressed, then the value of that key is stored in Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, Key");
                            clock.Running = false;
                            while (state.Keys == 0)
                            {
                                Thread.Sleep(1);
                            }

                            state.Registers[instruction.X] = (byte)state.GetKey();
                            state.ProgramCounter += 2;
                            clock.Running = true;
                            break;

                        case 0x18:
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tSOUND, V{instruction.X:X1}");
                            // Set sound timer = Vx.
                            // ST is set equal to the value of Vx.

                            var length = state.Registers[instruction.X] / 60f;
                            Console.Beep(500, (int)TimeSpan.FromSeconds(length).TotalMilliseconds);
                            state.ProgramCounter += 2;
                            break;

                        case 0x1E:
                            // Adds VX to I
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tADI\tV{instruction.X:X1}({state.Registers[instruction.X]})");
                            state.Index += state.Registers[instruction.X];
                            state.ProgramCounter += 2;
                            break;

                        case 0x29:
                            // Sets I to the location of the sprite for the character in VX. 
                            // Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLDI\tV{instruction.X:X1}({state.Registers[instruction.X]})");
                            state.Index = state.Memory[5 * state.Registers[instruction.X]];
                            state.ProgramCounter += 2;
                            break;

                        case 0x33:
                            // Store BCD representation of Vx in memory locations I, I + 1, and I+2.
                            // The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, 
                            // the tens digit at location I+1, and the ones digit at location I + 2.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tB, V{instruction.X:X1}({state.Registers[instruction.X]})");
                            state.Memory[state.Index] = (byte)(state.Registers[instruction.X] / 100);
                            state.Memory[state.Index + 1] = (byte)((state.Registers[instruction.X] / 10) % 10);
                            state.Memory[state.Index + 2] = (byte)(state.Registers[instruction.X] % 10);
                            state.ProgramCounter += 2;
                            break;

                        case 0x55:
                            // The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\t[I], V0-V{instruction.X:X1}({state.Registers[instruction.X]})");
                            for (var n = 0; n <= instruction.X; n++)
                            {
                                state.Memory[state.Index + n] = state.Registers[n];
                            }
                            state.ProgramCounter += 2;
                            break;

                        case 0x65:
                            // The interpreter reads values from memory starting at location I into registers V0 through Vx.
                            Console.WriteLine($"{state.ProgramCounter:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}({state.Registers[instruction.X]}), [I]");
                            for (var n = 0; n <= instruction.X; n++)
                            {
                                state.Registers[n] = state.Memory[state.Index + n];
                            }
                            state.ProgramCounter += 2;
                            break;

                        default:
                            throw new InvalidOperationException($"{instruction.Value:X4} NOT AN INSTRUCTION");
                    }
                    break;

                default:
                    throw new InvalidOperationException($"{instruction.Value:x4} NOT AN INSTRUCTION");
            }
        }

        public void Tick60Hz()
        {
            if (state.DelayTimer > 0)
            {
                state.DelayTimer -= 1;
            }
        }

        /// <summary>
        /// Loads a rom into memory at 512(0x200).
        /// </summary>
        /// <param name="file">The rom file path.</param>
        /// <returns>The size in bytes of the rom loaded.</returns>
        /// <remarks>Sets the Instruction Pointer to 512(0x200).</remarks>
        internal int Load(string file)
        {
            var clockMode = clock.Running;
            clock.Running = false;

            var programSize = 0;
            using (var stream = new FileStream(file, FileMode.Open))
            {
                state.ProgramCounter = 0x0200;
                programSize = stream.Read(state.Memory.AsSpan()[0x200..]);
            }

            clock.Running = clockMode;
            return programSize;
        }

        internal void Pause()
        {
            clock.Running = false;
        }

        internal void Resume()
        {
            clock.Running = true;
        }

        public static void LoadFonts(State state)
        {
            var fonts = new ulong[]
            {
                0xF0909090F0206020,
                0x2070F010F080F0F0,
                0x10F010F09090F010,
                0x10F080F010F0F080,
                0xF090F0F010204040,
                0xF090F090F0F090F0,
                0x10F0F090F09090E0,
                0x90E090E0F0808080,
                0xF0E0909090E0F080,
                0xF080F0F080F08080
            };

            fonts.CopyTo(MemoryMarshal.Cast<byte, ulong>(state.Memory.AsSpan()[0x00..0x50]));
        }
    }
}