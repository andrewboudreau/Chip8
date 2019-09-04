using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Chip8
{
    public class State
    {
        public readonly byte[] Memory = new byte[4096];

        public readonly byte[] Registers = new byte[16];

        public byte StackPointer { get; set; } = 0;

        public short Index { get; set; }

        public short InstructionPointer { get; set; }

        public byte DelayTimer { get; set; }

        public byte SoundTimer { get; set; }


        public Span<long> Display
        {
            get
            {
                return MemoryMarshal.Cast<byte, long>(Memory.AsSpan()[0x0F00..0x1000]);
            }
        }

        public Span<short> Stack
        {
            get
            {
                return MemoryMarshal.Cast<byte, short>(Memory.AsSpan()[0xEA0..0xF00]);
            }
        }
    }
}
