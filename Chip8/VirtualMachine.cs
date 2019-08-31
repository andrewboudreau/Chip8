using System;
using System.Collections;
using System.IO;
using System.Text;

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
            //Console.WriteLine($"OPCODE={instruction.Value:X4} Code={instruction.Code:X1} IP=0x{state.InstructionPointer:X4} Index=0x{state.Index:X4}");

            switch (instruction.Code)
            {
                case 0x0:
                    if (instruction.LowByte == 0xE0)
                    {
                        Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tCLS");
                        state.Display.Fill(0x0);
                        state.InstructionPointer += 2;
                        break;
                    }
                    else if (instruction.LowByte == 0xEE)
                    {
                        Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tRTS");

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

                    throw new InvalidOperationException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:X2}");

                case 0x1:
                    Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tJMP\t#{instruction.Address:X3}");
                    state.InstructionPointer = instruction.Address;
                    break;

                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                    Console.WriteLine($" Not Implemented");
                    break;

                case 0x6:
                    Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tV{instruction.Register:X1}, #{instruction.LowByte:X2}");

                    state.Registers[instruction.Register] = instruction.LowByte;
                    state.InstructionPointer += 2;
                    break;

                case 0x7:
                    Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tADD\tV{instruction.Register:X1}, #{instruction.LowByte:X2}");
                    state.Registers[instruction.Register] += instruction.LowByte;
                    state.InstructionPointer += 2;
                    break;

                case 0x8:
                case 0x9:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:X2}");
                    break;

                case 0xA:
                    Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tI, #{instruction.Address:X3}");
                    state.Index = instruction.Address;
                    state.InstructionPointer += 2;
                    break;

                case 0xB:
                case 0xC:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:X2}");
                    break;

                case 0xD:
                    var x = state.Registers[instruction.Nibs[1]];
                    var y = state.Registers[instruction.Nibs[2]];

                    Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tDRW\tV{instruction.Nibs[1]:X1}({x}), V{instruction.Nibs[2]:X1}({y}), {instruction.Nibs[3]}");
                    for (var n = 0; n < instruction.Nibs[3]; n++)
                    {
                        var offset = (64 * ((y-1) + n)) + (x-1);
                        Console.WriteLine($"n={n} x={x} y={y} offset%8={offset % 8} offset/8={offset / (float)8} offset={offset} (y+n)={y + n} (8*(y+n))={8 * (y + n)}");
                        //this clears the stuff that overlaps =(
                        state.Display[offset / 8] = (byte)(state.Memory[state.Index + n] << (offset % 8));
                        state.Display[(offset / 8)+1] = (byte)(state.Memory[state.Index + n] >> (offset % 8));
                    }

                    state.InstructionPointer += 2;
                    break;

                case 0xE:
                case 0xF:
                    Console.WriteLine($" Not Implemented");
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:X2}");
                    break;

                default:
                    throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:X2}");
            }

            //Console.WriteLine(string.Empty);
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
                Console.Write($"{state.Memory[i]:X2} ");

                if ((i + 1) % 16 == 0)
                {
                    Console.WriteLine(string.Empty);
                }
            }

            Console.WriteLine(string.Empty);
        }

        internal string DumpMemoryString(Range range)
        {
            var buffer = new StringBuilder();
            var line = 0;

            buffer.Append($"| {range.Start.Value:X4} ");
            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                buffer.Append($"{state.Memory[i]:X2} ");

                if (i % 16 == 0)
                {
                    buffer.AppendLine();
                    buffer.Append($"| {i + (line++ * 16):X4} ");
                }
            }

            if ((range.End.Value - range.Start.Value) % 16 != 0)
            {
                buffer.AppendLine();
            }

            return buffer.ToString();
        }

        internal string DumpRegisterString()
        {
            var buffer = new StringBuilder();

            buffer.Append($"| Index = #{state.Index:X3} ");
            buffer.AppendLine($"  IP = #{state.InstructionPointer:X3} ");

            for (var i = 0; i < 8; i++)
            {
                buffer.AppendLine($"| V{i:X1} = #{state.Registers[i]:X2}\tV{i + 8:X1} = #{state.Registers[i + 8]:X2}");
            }

            return buffer.ToString();
        }

        public void Render()
        {
            var i = 0;
            var bitArray = new BitArray(state.Display.ToArray());
            foreach (var bit in bitArray)
            {
                Console.Write((bool)bit ? "█" : $"{i % 8}");
                if ((i + 1) % 64 == 0)
                {
                    Console.WriteLine();
                }

                i++;
            }
        }

        public void Render2()
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
