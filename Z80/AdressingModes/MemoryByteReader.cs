using System;
namespace Z80.AddressingModes
{
    public class MemoryByteReader : IReadAddressedOperand<byte> 
    {
        private readonly MemReadCycle _memoryReader;

        private readonly InternalCycle _internalCycle; 

        public MemoryByteReader(Z80Cpu cpu, ushort? address = null, bool additionalCycleAtEnd = false) {
            _memoryReader = new MemReadCycle(cpu);
            _memoryReader.Address = address;
            _internalCycle = new InternalCycle(additionalCycleAtEnd ? 1 : 0);
        }

        public byte AddressedValue => _memoryReader.LatchedData;

        public bool IsComplete => _memoryReader.IsComplete && _internalCycle.IsComplete;

        public void Clock()
        {
            if (!_memoryReader.IsComplete) {
                _memoryReader.Clock();
                return;
            }
            if (!_internalCycle.IsComplete) {
                _internalCycle.Clock();
                return;
            }
            throw new InvalidOperationException();
        }

        public void Reset()
        {
            _memoryReader.Reset();
            _internalCycle.Reset();
        }
    }
}