
using System;

namespace Z80.AddressingModes
{

    public class ExtendedAddressMode : IAddressMode<ushort> {
        private Z80Cpu _cpu;

        public ExtendedAddressMode(Z80Cpu cpu)
        {
            _cpu = cpu;
        } 

        public IReadAddressedOperand<ushort> Reader => new MemoryShortReader(_cpu);

        public IWriteAddressedOperand<ushort> Writer => throw new InvalidOperationException("The address mode is read only");

        public bool IsComplete => true;

        public string Description => "nn";

        public void Clock()
        {
        }

        public void Reset()
        {
        }
    }
}