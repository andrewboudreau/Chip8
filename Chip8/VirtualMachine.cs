using System;
using System.Collections;
using System.IO;

namespace Chip8
{
    public class VirtualMachine
    {
        private readonly State state;

        public VirtualMachine(State state)
        {
            this.state = state;
        }

        public void Invoke(ushort op)
        {
            var register = (op & 0x0F00) >> 8;
            var left = (op & 0x00F0) >> 4;
            var right = (op & 0x000F);

            var highnib = op >> 12;
            Console.WriteLine($"OPCODE=0x{op:x4}, IP=0x{state.InstructionPointer:X4}, Index=0x{state.Index:X4}");

            switch (highnib)
            {
                case 0x0:
                    if ((op & 0X00FF) == 0xE0)
                    {
                        Console.WriteLine($" CLS");
                        state.Display.Fill(0x0);
                        state.InstructionPointer += 2;
                        break;
                    }
                    else if ((op & 0X00FF) == 0xEE)
                    {
                        Console.WriteLine(" RTS (Return-To-Sender)");

                        if (state.Stack.Count == 0)
                        {
                            throw new Exception(" Program Halted.");
                            //// return;
                        }

                        Console.WriteLine($" Returning from IP=0x{state.InstructionPointer:X3}");
                        state.InstructionPointer = state.Stack.Pop();
                        Console.WriteLine($" Returned to IP=0x{state.InstructionPointer:X3}");
                        break;
                    }

                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");

                case 0x1:
                    Console.WriteLine($"Jump to {op & 0x0FFF} (0x{(ushort)(op & 0x0FFF):X3})");
                    state.InstructionPointer = (ushort)(op & 0x0FFF);
                    break;

                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                    Console.WriteLine($" Not Implemented");
                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");
                    break;

                case 0x6:
                    Console.WriteLine($" Updating Register[V{register:X1}]=0x{right:X2}");
                    state.Registers[register] = (byte)right;
                    Console.WriteLine($" Updated Register[V{register:X1}]=0x{right:X2}");
                    state.InstructionPointer += 2;
                    break;

                case 0x7:
                    Console.WriteLine($" Updating Register[V{register:X1}]={state.Registers[register]}(0x{state.Registers[register]:x2})");
                    state.Registers[register] += (byte)right;
                    Console.WriteLine($" Updated Register[V{register:X1}]={state.Registers[register]}(0x{state.Registers[register]:x2})");
                    state.InstructionPointer += 2;
                    break;

                case 0x8:
                case 0x9:
                    Console.WriteLine($" Not Implemented");
                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");
                    break;

                case 0xA:
                    Console.WriteLine($"  Updating Index=0x{state.Index:X3}");
                    state.Index = (ushort)(op & 0x0FFF);
                    Console.WriteLine($"  Updated Index=0x{state.Index:X3}");
                    state.InstructionPointer += 2;
                    break;

                case 0xB:
                case 0xC:
                    Console.WriteLine($" Not Implemented");
                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");
                    break;

                case 0xD:
                    Console.WriteLine($" Draw (Vx=0x{register:X1}, Vy=0x{left:X1}, N={right:x1} at Memory={state.Index:x3}");
                    state.InstructionPointer += 2;
                    break;

                case 0xE:
                case 0xF:
                    Console.WriteLine($" Not Implemented");
                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");
                    break;

                default:
                    throw new InvalidOperationException($" register=0x{register:X2} args=0x{right:x2}");
            }

            Console.WriteLine(string.Empty);
        }

        internal void EmulateOne()
        {
            var high = state.Memory[state.InstructionPointer];
            var low = state.Memory[state.InstructionPointer + 1];

            Invoke((ushort)((high << 8) + low));
        }

        internal void Load(string file)
        {
            var sizeInBytes = LoadHex(file);
            state.InstructionPointer = 0x0200;

            Console.WriteLine($"Loaded '{file}' as {sizeInBytes:N0} bytes.");
        }

        private int LoadHex(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open))
            {
                return stream.Read(state.Memory.AsSpan()[0x200..]);
            }
        }

        internal void DumpMemory(Range range)
        {
            Console.WriteLine($"Dumping Memory {range.Start.Value}(0x{range.Start.Value:X4}).. to {range.End.Value}(0x{range.End.Value:X4})");

            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                Console.Write($"{state.Memory[i]:x2} ");

                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine(string.Empty);
                }
            }
              
            Console.WriteLine(string.Empty);
        }

        public void Render()
        {
            Console.WriteLine(string.Empty.PadLeft(64, '-'));
            var i = 0;

            var bitArray = new BitArray(state.Display.ToArray());
            Console.Write("0 - ".PadLeft(9, ' '));
            foreach (var bit in bitArray)
            {
                Console.Write((bool)bit ? "█" : " ");
                if ((++i) % 64 == 0)
                {
                    Console.WriteLine();
                    Console.Write($"{i} - ".PadLeft(9, ' '));
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine("".PadLeft(64, '-'));
        }
    }
}
