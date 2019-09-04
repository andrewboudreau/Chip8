using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Chip8
{
	public class VirtualMachine
	{
		private readonly Random rand = new Random(0);
		private readonly State state;
		private readonly IScreen screen;

		public VirtualMachine(State state, IScreen screen)
		{
			this.state = state;
			this.screen = screen;

			LoadFonts(this.state);
		}

		public void EmulateOne()
		{
			var high = state.Memory[state.InstructionPointer];
			var low = state.Memory[state.InstructionPointer + 1];

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
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tCLS");
							state.ScreenBuffer.Fill(0x0);
							state.InstructionPointer += 2;

							screen.Update();
							break;

						case 0xEE:
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tRTS");

							if (state.StackPointer == 0)
							{
								throw new Exception(" Program Halted.");
							}

							state.StackPointer--;
							state.InstructionPointer = state.Stack[state.StackPointer];
							state.InstructionPointer += 2;

							break;

						default:
							throw new InvalidOperationException($" register=0x{instruction.X:X2} args=0x{instruction.X:X2}");
					}

					break;
				case 0x1:
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tJMP\t#{instruction.NNN:X3}");
					state.InstructionPointer = instruction.NNN;
					break;

				case 0x2:
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tCALL\t#{instruction.NNN:X3}");
					state.Stack[state.StackPointer] = state.InstructionPointer;
					state.StackPointer++;
					state.InstructionPointer = instruction.NNN;
					break;

				case 0x3:
					// Skips the next instruction if VX equals NN.
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSE\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
					if (state.Registers[instruction.X] == instruction.NN)
					{
						state.InstructionPointer += 2;
					}

					state.InstructionPointer += 2;
					break;

				case 0x4:
					// Skips the next instruction if VX doesn't equal NN.
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSNE\tV{instruction.X:X1}({state.Registers[instruction.X]}), #{instruction.NN:X3}");
					if (state.Registers[instruction.X] != instruction.NN)
					{
						state.InstructionPointer += 2;
					}

					state.InstructionPointer += 2;
					break;

				case 0x5:
					// Skips the next instruction if VX equals VY.
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSE\tV{instruction.X:X1}({state.Registers[instruction.X]}), V{instruction.Y:X1}({state.Registers[instruction.Y]})");
					if (state.Registers[instruction.X] == state.Registers[instruction.Y])
					{
						state.InstructionPointer += 2;
					}

					state.InstructionPointer += 2;
					break;

				case 0x6:
					// Load NN into RX
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, #{instruction.NN:X2}");
					state.Registers[instruction.X] = instruction.NN;
					state.InstructionPointer += 2;
					break;

				case 0x7:
					// Adds NN to VX. (Carry flag is not changed)
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tADD\tV{instruction.X:X1}, #{instruction.NN:X2}");
					state.Registers[instruction.X] += instruction.NN;
					state.InstructionPointer += 2;
					break;

				case 0x8:
					switch (instruction.N)
					{
						case 0x0:
							//Stores the value of register Vy in register Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[instruction.X] = state.Registers[instruction.Y];
							break;

						case 0x1:
							// Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tOR\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[instruction.X] |= state.Registers[instruction.Y];
							break;

						case 0x2:
							// Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tAND\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[instruction.X] &= state.Registers[instruction.Y];
							break;

						case 0x3:
							// Performs a bitwise XOR on the values of Vx and Vy, then stores the result in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tAND\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[instruction.X] ^= state.Registers[instruction.Y];
							break;

						case 0x4:
							// The values of Vx and Vy are added together. 
							// If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. 
							// Only the lowest 8 bits of the result are kept, and stored in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tADD\tV{instruction.X:X1}, V{instruction.Y:X2}(VF={state.Registers[0xF]})");
							state.Registers[0xF] = (byte)(state.Registers[instruction.X] + state.Registers[instruction.Y] > 0xFFFF ? 1 : 0);
							state.Registers[instruction.X] += state.Registers[instruction.Y];
							break;

						case 0x5:
							// VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
							// If Vx > Vy, then VF is set to 1, otherwise 0.
							// Then Vy is subtracted from Vx, and the results stored in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSUB\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[0xF] = (byte)(state.Registers[instruction.X] > state.Registers[instruction.Y] ? 1 : 0);
							state.Registers[instruction.X] -= state.Registers[instruction.Y];
							break;

						case 0x6:
							// Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSHR\tV{instruction.X:X1}");
							state.Registers[0xF] = (byte)(state.Registers[instruction.X] & 0x01);
							state.Registers[instruction.X] = (byte)(state.Registers[instruction.X] >> 1);
							break;

						case 0x7:
							// If Vy > Vx, then VF is set to 1, otherwise 0.
							// Then Vx is subtracted from Vy, and the results stored in Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSUBN\tV{instruction.X:X1}, V{instruction.Y:X2}");
							state.Registers[0xF] = (byte)(state.Registers[instruction.Y] > state.Registers[instruction.X] ? 1 : 0);
							state.Registers[instruction.X] -= state.Registers[instruction.Y];
							break;

						case 0xE:
							// If the most - significant bit of Vx is 1, then VF is set to 1, otherwise to 0.
							// Then Vx is multiplied by 2.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSHL\tV{instruction.X:X1}");
							state.Registers[0xF] = (byte)((state.Registers[instruction.X] & (1 << 7)) >> 7);
							state.Registers[instruction.X] = (byte)(state.Registers[instruction.X] << 1);
							break;

						default:
							throw new InvalidOperationException($"{instruction.Value:x4} NOT AN INSTRUCTION");
					}

					state.InstructionPointer += 2;
					break;

				case 0x9:
					// Skips the next instruction if VX doesn't equal VY.  
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tSNE\tV{instruction.X:X1}({state.Registers[instruction.X]}), V{instruction.Y:X1}({state.Registers[instruction.Y]})");
					if (state.Registers[instruction.X] != state.Registers[instruction.Y])
					{
						state.InstructionPointer += 2;
					}

					state.InstructionPointer += 2;
					break;

				case 0xA:
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tI, #{instruction.NNN:X3}");
					state.Index = instruction.NNN;
					state.InstructionPointer += 2;
					break;

				case 0xB:
					// Jumps to the address NNN plus V0.
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tJMI\t#{instruction.NNN:X3}, V0({state.Registers[0]}");
					state.InstructionPointer = (ushort)(instruction.NNN + state.Registers[0]);
					break;

				case 0xC:
					// register VX = random number AND KK
					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tRND\tV{instruction.X:X1}, #{instruction.NN:x2}");
					state.Registers[instruction.X] = (byte)(rand.Next(0, 256) & instruction.NN);
					state.InstructionPointer += 2;
					break;

				case 0xD:
					// Draw sprite for memory location to screen memory at X,Y screen coordinates.
					var x = state.Registers[instruction.X];
					var y = state.Registers[instruction.Y];

					Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tDRW\tV{instruction.X:X1}({x}), V{instruction.Y:X1}({y}), {instruction.Nibs[3]}");
					for (var n = 0; n < instruction.N; n++)
					{
						// xor data from memory into screen memory
						state.ScreenBuffer[y + n] ^= (ulong)state.Memory[state.Index + n] << (64 - 8 - x);

						// todo: Wrap pixels around edges of screen
						// todo: Set VF if pixels are toggled
					}

					state.InstructionPointer += 2;

					screen.Update();
					break;

				case 0xE:
					throw new InvalidOperationException($"{instruction.Value:X4} NOT AN INSTRUCTION");
					break;

				case 0xF:
					switch (instruction.NN)
					{
						case 0x07:
							// Sets VX to the value of the delay timer.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}, DELAY");
							state.Registers[instruction.X] = state.DelayTimer;
							state.InstructionPointer += 2;
							break;

						case 0x15:
							// Sets the delay timer to VX.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tDELAY, V{instruction.X:X1}");
							state.DelayTimer = 0;//state.Registers[instruction.X];
							state.InstructionPointer += 2;
							break;

						case 0x1E:
							// Adds VX to I
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tADI\tV{instruction.X:X1}({state.Registers[instruction.X]})");
							state.Index += state.Registers[instruction.X];
							state.InstructionPointer += 2;
							break;

						case 0x29:
							// Sets I to the location of the sprite for the character in VX. 
							// Characters 0-F (in hexadecimal) are represented by a 4x5 font.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLDI\tV{instruction.X:X1}({state.Registers[instruction.X]})");
							state.Index = state.Memory[5 * (int)(state.Registers[instruction.X])];
							state.InstructionPointer += 2;
							break;

						case 0x33:
							// Store BCD representation of Vx in memory locations I, I + 1, and I+2.
							// The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, 
							// the tens digit at location I+1, and the ones digit at location I + 2.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tB, V{instruction.X:X1}({state.Registers[instruction.X]})");
							state.Memory[state.Index] = (byte)(state.Registers[instruction.X] / 100);
							state.Memory[state.Index + 1] = (byte)((state.Registers[instruction.X] / 10) % 10);
							state.Memory[state.Index + 2] = (byte)(state.Registers[instruction.X] % 10);
							state.InstructionPointer += 2;
							break;

						case 0x55:
							// The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\t[I], V0-V{instruction.X:X1}({state.Registers[instruction.X]})");
							for (var n = 0; n <= instruction.X; n++)
							{
								state.Memory[state.Index + n] = state.Registers[n];
							}
							state.InstructionPointer += 2;
							break;

						case 0x65:
							// The interpreter reads values from memory starting at location I into registers V0 through Vx.
							Console.WriteLine($"{state.InstructionPointer:X4} - {instruction.Value:X4}\tLD\tV{instruction.X:X1}({state.Registers[instruction.X]}), [I]");
							for (var n = 0; n <= instruction.X; n++)
							{
								state.Registers[n] = state.Memory[state.Index + n];
							}
							state.InstructionPointer += 2;
							break;

						default:
							throw new InvalidOperationException($"{instruction.Value:X4} NOT AN INSTRUCTION");
					}

					break;

				default:
					throw new InvalidOperationException($"{instruction.Value:x4} NOT AN INSTRUCTION");
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
			using (var stream = new FileStream(file, FileMode.Open))
			{
				state.InstructionPointer = 0x0200;
				return stream.Read(state.Memory.AsSpan()[0x200..]);
			}
		}

		internal string DumpMemoryString(Range range)
		{
			var buffer = new StringBuilder();
			var line = 0;

			buffer.Append($"|{range.Start.Value:X4} ");
			for (var i = range.Start.Value; i < range.End.Value; i++)
			{
				buffer.Append($"{state.Memory[i]:X2} ");

				if ((i + 1) % 16 == 0)
				{
					buffer.AppendLine();
					buffer.Append($"|{i + (line++ * 16):X4} ");
				}
			}

			if ((range.End.Value - range.Start.Value) % 16 != 0)
			{
				//buffer.AppendLine();
			}

			return buffer.ToString();
		}

		internal string DumpRegisterString()
		{
			var buffer = new StringBuilder();

			buffer.Append($"| Index = #{state.Index:X3} ");
			buffer.AppendLine($"IP = #{state.InstructionPointer:X3} ");

			for (var i = 0; i < 16; i++)
			{
				buffer.AppendLine($"| V{i:X1} = #{state.Registers[i]:X2}");
			}

			return buffer.ToString();
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