﻿namespace Chip8
{
	public ref struct Instruction
	{
		public Instruction(byte high, byte low)
		{
			HighByte = high;
			LowByte = low;
			Nibs = new byte[]
			{
				(byte)((HighByte & 0xF0) >> 4),
				(byte)(HighByte & 0x0F),
				(byte)((LowByte & 0xF0) >> 4),
				(byte)(LowByte & 0x0F)
			};
		}

		public byte[] Nibs;
		public byte HighByte;
		public byte LowByte;

		public ushort Value => (ushort)((HighByte << 8) + LowByte);

		public ushort Address => (ushort)(Value & 0X0FFF);

		public byte Code => Nibs[0];

		public byte Register => Nibs[1];

		public byte Operand1 => Nibs[2];

		public byte Operand2 => Nibs[3];

		public override string ToString()
		{
			return $"0x{Value.ToString("x4").ToUpper()} ";
		}
	}
}