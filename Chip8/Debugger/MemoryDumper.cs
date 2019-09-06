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

		internal static string DumpRegisterString(this State state)
		{
			var buffer = new StringBuilder();

			buffer.Append($"| Index = #{state.Index:X3} ");
			buffer.AppendLine($"IP = #{state.ProgramCounter:X3} ");

			for (var i = 0; i < 16; i++)
			{
				buffer.AppendLine($"| V{i:X1} = #{state.Registers[i]:X2}");
			}

			return buffer.ToString();
		}
	}
}
