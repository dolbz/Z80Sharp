using System;
using Z80;
using Z80.AddressingModes;

public class RelativeReader : IReadAddressedOperand<ushort>
    {
        private Z80Cpu _cpu;
        private sbyte _offset;

        public ushort AddressedValue => (ushort)(_cpu.PC + _offset);

        public bool IsComplete => true;

        public RelativeReader(Z80Cpu cpu, sbyte offset) {
            _cpu = cpu;
            _offset = offset;
        }

        public void Clock()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
        }
    }