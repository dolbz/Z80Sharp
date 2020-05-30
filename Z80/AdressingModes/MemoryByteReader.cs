namespace Z80.AddressingModes
{
    public class MemoryByteReader : IReadAddressedOperand<byte> 
    {
        private readonly MemReadCycle _memoryReader;

        public MemoryByteReader(Z80Cpu cpu, ushort? address = null) {
            _memoryReader = new MemReadCycle(cpu);
            _memoryReader.Address = address;
        }

        public byte AddressedValue => _memoryReader.LatchedData;

        public bool IsComplete => _memoryReader.IsComplete;

        public void Clock()
        {
            _memoryReader.Clock();
        }

        public void Reset()
        {
            _memoryReader.Reset();
        }
    }
}