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

        public void Execute(Instruction instruction)
        {
            Console.WriteLine($"OPCODE=0x{instruction.Value:x4}, IP=0x{state.InstructionPointer:X4}, Index=0x{state.Index:X4}");

            switch (instruction.Code)
            {
                case 0x0:
                    if (instruction.Operand2 == 0xE0)
                    {
                        Console.WriteLine($" CLS");
                        state.Display.Fill(0x0);
                        state.InstructionPointer += 2;
                        break;
                    }
                    else if (instruction.Operand2 == 0xEE)
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

                    throw new InvalidOperationException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");

                case 0x1:
                    Console.WriteLine($"Jump to {instruction.Address} (0x{instruction.Address:X3})");
                    state.InstructionPointer = instruction.Address;
                    break;

                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                    Console.WriteLine($" Not Implemented");
                    break;

                case 0x6:
                    Console.WriteLine($" Updating Register[V{instruction.Register:X1}]=0x{instruction.Operand2:X2}");
                    state.Registers[instruction.Register] = instruction.Operand2;
                    Console.WriteLine($" Updated Register[V{instruction.Register:X1}]=0x{instruction.Operand2:X2}");
                    state.InstructionPointer += 2;
                    break;

                case 0x7:
                    Console.WriteLine($" Updating Register[V{instruction.Register:X1}]={state.Registers[instruction.Register]}(0x{state.Registers[instruction.Register]:x2})");
                    state.Registers[instruction.Register] += instruction.Operand2;
                    Console.WriteLine($" Updated Register[V{instruction.Register:X1}]={state.Registers[instruction.Register]}(0x{state.Registers[instruction.Register]:x2})");
                    state.InstructionPointer += 2;
                    break;

                case 0x8:
                case 0x9:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
                    break;

                case 0xA:
                    Console.WriteLine($"  Updating Index=0x{state.Index:X3}");
                    state.Index = instruction.Address;
                    Console.WriteLine($"  Updated Index=0x{state.Index:X3}");
                    state.InstructionPointer += 2;
                    break;

                case 0xB:
                case 0xC:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
                    break;

                case 0xD:
                    Console.WriteLine($" Draw (Vx=0x{instruction.Register:X1}, Vy=0x{instruction.Operand1:X1}, N={instruction.Operand2:x1} at Memory={state.Index:x3}");
                    state.InstructionPointer += 2;
                    break;

                case 0xE:
                case 0xF:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
                    break;

                default:
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
            }

            Console.WriteLine(string.Empty);
        }

        internal void EmulateOne()
        {
            var high = state.Memory[state.InstructionPointer];
            var low = state.Memory[state.InstructionPointer + 1];

            Execute(new Instruction(high, low));
        }

        /// <summary>
        /// Loads a rom into memory at 512(0x200).
        /// </summary>
        /// <param name="file">The rom file path.</param>
        /// <returns>The size in bytes of the rom loaded.</returns>
        /// <remarks>Sets the Instruction Pointer to 512(0x200).</remarks>
        internal int Load(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open))
            {
                state.InstructionPointer = 0x0200;
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

    public ref struct Instruction
    {
        public Instruction(byte high, byte low)
        {
            Value = (ushort)((high << 8) + low);
            Code = (byte)(high & 0xF0);
            Address = (ushort)(Value & 0X0FFF);
            Register = (byte)(high & 0x0F);
            Operand1 = (byte)(low & 0xF0);
            Operand2 = (byte)(low & 0x0F);
            HighByte = high;
            LowByte = low;
        }

        public ushort Value;
        public byte Code;
        public ushort Address;
        public byte Register;
        public byte Operand1;
        public byte Operand2;
        public byte HighByte;
        public byte LowByte;

        public override string ToString()
        {
            return $"0x{Value.ToString("x4").ToUpper()}";
        }
    }
}
