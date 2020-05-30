namespace Z80.AddressingModes
{
    public class MemoryShortReader : IReadAddressedOperand<ushort> 
    {
        private readonly MemReadCycle _byte1Reader;
        private readonly MemReadCycle _byte2Reader;

        public MemoryShortReader(Z80Cpu cpu, ushort? address = null) {
            _byte1Reader = new MemReadCycle(cpu);
            _byte1Reader.Address = address;
            _byte2Reader = new MemReadCycle(cpu);
            _byte2Reader.Address = (address == null ? (ushort?)null : (ushort)(address+1));
        }

        public ushort AddressedValue => (ushort)(_byte2Reader.LatchedData << 8 | _byte1Reader.LatchedData);

        public bool IsComplete => _byte2Reader.IsComplete;

        public void Clock()
        {
            if (!_byte1Reader.IsComplete) {
                _byte1Reader.Clock();
                return;
            }
            if (!_byte2Reader.IsComplete) {
                _byte2Reader.Clock();
                return;
            }
        }

        public void Reset()
        {
            _byte1Reader.Reset();
            _byte2Reader.Reset();
        }
    }
}