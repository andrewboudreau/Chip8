using System;
using System.Collections.Generic;

namespace Chip8
{
    public class State
    {
        public readonly byte[] Memory = new byte[4096];

        public readonly byte[] Registers = new byte[16];

        public readonly Stack<ushort> Stack = new Stack<ushort>(24);

        public ushort Index { get; set; }

        public ushort InstructionPointer { get; set; }

        public byte DelayTimer { get; private set; }

        public byte SoundTimer { get; private set; }

        public Span<byte> Display
        {
            get
            {
                return Memory.AsSpan()[0x0F00..0x0FFF];
            }
        }

        public Span<byte> CallStack
        {
            get
            {
                return Memory.AsSpan()[0xEA0..0xEFF];
            }
        }
    }
}
