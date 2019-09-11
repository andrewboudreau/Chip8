using System;
using System.Runtime.InteropServices;

namespace Chip8
{
    public class State
    {
        public readonly byte[] Memory = new byte[4096];

        public readonly byte[] Registers = new byte[16];

        public ushort ProgramCounter { get; set; }

        public byte StackPointer { get; set; } = 0;

        public ushort Index { get; set; }

        public byte DelayTimer { get; set; }

        public byte SoundTimer { get; set; }

        public Span<ulong> ScreenBuffer
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

        /// <summary>
        /// 1-bit for each input 1 if pressed, otherwise 0.
        /// </summary>
        public ushort Keys { get; set; }

        public void KeyPress(int key)
        {
            if (key < 0 || key > 0xF)
                throw new InvalidOperationException($"Invalid key pressed, '{key}'.");

            Keys |= (ushort)(1 << key);
        }

        public void KeyRelease(int key)
        {
            if (key < 0 || key > 0xF)
                throw new InvalidOperationException($"Invalid key released, '{key}'.");

            Keys &= (ushort)~(1 << key);
        }

        public int GetKey()
        {
            for (var i = 0; i <= 0xF; i++)
            {
                if ((Keys & (1 << i)) > 0)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
