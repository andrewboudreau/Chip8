namespace Chip8
{
	public ref struct Instruction
	{
		public Instruction(byte high, byte low)
		{
			HighByte = high;
			LowByte = low;
			Value = (ushort)((HighByte << 8) + LowByte);
			Code = (byte)((HighByte & 0xF0) >> 4);
			X = (byte)(HighByte & 0x0F);
			Y = (byte)((LowByte & 0xF0) >> 4);
			N = (byte)(LowByte & 0x0F);
			NN = LowByte;
			NNN = (ushort)(((HighByte << 8) + LowByte) & 0X0FFF);
		}

		public byte HighByte;
		public byte LowByte;

		public ushort Value;
		public byte Code;
		public byte X;
		public byte Y;
		public byte N;
		public byte NN;
		public ushort NNN;

		public override string ToString()
		{
			return $"0x{Value.ToString("X4")} ";
		}
	}
}
