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

        public ushort Index { get; set; }

        public ushort InstructionPointer { get; set; }

        public byte DelayTimer { get; set; }

        public byte SoundTimer { get; set; }


        public Span<ulong> Display
        {
            get
            {
                return MemoryMarshal.Cast<byte, ulong>(Memory.AsSpan()[0x0F00..0x1000]);
            }
        }

        public Span<ushort> Stack
        {
            get
            {
                return MemoryMarshal.Cast<byte, ushort>(Memory.AsSpan()[0xEA0..0xF00]);
            }
        }
    }
}
