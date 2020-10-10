using System;

namespace Z80.AddressingModes {
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

    public class RelativeAddressMode : IAddressMode<ushort>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemoryByteReader _offsetReader;
    
        public IReadAddressedOperand<ushort> Reader => new RelativeReader(_cpu, (sbyte)_offsetReader.AddressedValue);

        public IWriteAddressedOperand<ushort> Writer => throw new InvalidOperationException("Cannot write with the relative addressing mode");

        public bool IsComplete => _offsetReader.IsComplete;

        public RelativeAddressMode(Z80Cpu cpu) {
            _cpu = cpu;
            _offsetReader = new MemoryByteReader(cpu);
        }

        public void Clock()
        {
            if (!_offsetReader.IsComplete) {
                _offsetReader.Clock();
                return;
            }
        }

        public void Reset()
        {
            _offsetReader.Reset();
        }
    }
}