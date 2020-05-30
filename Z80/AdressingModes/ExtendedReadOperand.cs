
using System;

namespace Z80.AddressingModes
{

    public class ExtendedReadOperand : MemoryShortReader, IAddressMode<ushort> {
            public ExtendedReadOperand(Z80Cpu cpu) : base(cpu) 
            {
            }

        public IReadAddressedOperand<ushort> Reader => this;

        public IWriteAddressedOperand<ushort> Writer => throw new InvalidOperationException("The address mode is read only");
    }
}