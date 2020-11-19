namespace Z80.AddressingModes
{
    public struct ExtendedPointer16Bit : IAddressMode<ushort>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemoryShortReader _extendedOperand;

        public bool IsComplete => _extendedOperand.IsComplete;

        public IReadAddressedOperand<ushort> Reader => new MemoryShortReader(_cpu, _extendedOperand.AddressedValue);

        public IWriteAddressedOperand<ushort> Writer => new MemoryShortWriter(_cpu, _extendedOperand.AddressedValue, false);

        public string Description => "(nn)";

        public ExtendedPointer16Bit(Z80Cpu cpu)
        {
            _cpu = cpu;
            _extendedOperand = new MemoryShortReader(cpu);
        }

        public void Clock()
        {
            if (!_extendedOperand.IsComplete)
            {
                _extendedOperand.Clock();
            }
        }

        public void Reset()
        {
            _extendedOperand.Reset();
        }
    }
}