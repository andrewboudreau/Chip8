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
			Console.WriteLine($"OPCODE={instruction.Value:x4} Code={instruction.Code:x1} IP=0x{state.InstructionPointer:X4} Index=0x{state.Index:X4}");

			switch (instruction.Code)
			{
				case 0x0:
					if (instruction.LowByte == 0xE0)
					{
						Console.WriteLine($"0x{state.InstructionPointer:x4}\t{instruction.Value:x4}\t cls");
						state.Display.Fill(0x0);
						state.InstructionPointer += 2;
						break;
					}
					else if (instruction.LowByte == 0xEE)
					{
						Console.WriteLine($"0x{state.InstructionPointer:x4}\t{instruction.Value:x4}\t rts");

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

					Console.WriteLine($"0x{state.InstructionPointer:x4}\t{instruction.Value:x4}\t jmp {instruction.Address:X3}");
					state.InstructionPointer = instruction.Address;
					break;

				case 0x2:
				case 0x3:
				case 0x4:
				case 0x5:
					Console.WriteLine($" Not Implemented");
					break;

				case 0x6:
					Console.WriteLine($"0x{state.InstructionPointer:x4}\t{instruction.Value:x4}\t mov {instruction.Register:X1}, {instruction.LowByte:X2}");

					//Console.WriteLine($" Updating Register[V{instruction.Register:X1}]=0x{instruction.Operand2:X2}");
					state.Registers[instruction.Register] = instruction.LowByte;
					//Console.WriteLine($" Updated Register[V{instruction.Register:X1}]=0x{instruction.Operand2:X2}");
					state.InstructionPointer += 2;
					break;

				case 0x7:
					//Console.WriteLine($" Updating Register[V{instruction.Register:X1}]={state.Registers[instruction.Register]}(0x{state.Registers[instruction.Register]:x2})");
					state.Registers[instruction.Register] += instruction.Operand2;
					//Console.WriteLine($" Updated Register[V{instruction.Register:X1}]={state.Registers[instruction.Register]}(0x{state.Registers[instruction.Register]:x2})");
					state.InstructionPointer += 2;
					break;

				case 0x8:
				case 0x9:
					Console.WriteLine($" Not Implemented");
					throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
					break;

				case 0xA:
					Console.WriteLine($"0x{state.InstructionPointer:x4}\t{instruction.Value:x4}\t mvi {instruction.Address:X3}");

					//Console.WriteLine($"  Updating Index=0x{state.Index:X3}");
					state.Index = instruction.Address;
					//Console.WriteLine($"  Updated Index=0x{state.Index:X3}");
					state.InstructionPointer += 2;
					break;

				case 0xB:
				case 0xC:
					Console.WriteLine($" Not Implemented");
					throw new NotImplementedException($" register=0x{instruction.Register:X2} args=0x{instruction.Operand2:x2}");
					break;

				case 0xD:
					var x = state.Registers[instruction.Nibs[1]];
					var y = state.Registers[instruction.Nibs[2]];

					for (var n = 0; n < instruction.Nibs[3]; n++)
					{
						state.Display[0 + (0xFF * n)] = state.Memory[state.Index];
					}

					Console.WriteLine($"sprite {x}, {y}, {instruction.Nibs[3]}");
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

		internal string DumpMemoryString(Range range)
		{
			var buffer = new StringBuilder();
			var line = 0;

			buffer.Append($"| 0x{range.Start.Value:x4} ");
			for (var i = range.Start.Value; i < range.End.Value; i++)
			{
				buffer.Append($"{state.Memory[i]:x2} ");

				if ((i + 1) % 16 == 0)
				{
					buffer.AppendLine();
					buffer.Append($"| 0x{i + (line++ * 16):x4} ");
				}
			}

			if ((range.End.Value - range.Start.Value) % 16 != 0)
			{
				buffer.AppendLine();
			}

			return buffer.ToString();
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
