
using System;

namespace Z80.AddressingModes
{
    public class MemoryByteWriter : IWriteAddressedOperand<byte> 
    {
        private readonly MemWriteCycle _memoryWriter;

        public MemoryByteWriter(Z80Cpu cpu, ushort address) {
            _memoryWriter = new MemWriteCycle(cpu);
            _memoryWriter.Address = address;
        }

        public byte AddressedValue { set{ _memoryWriter.DataToWrite = value; } }

        public bool IsComplete => _memoryWriter.IsComplete;

        public bool WriteReady => throw new NotImplementedException(); // TODO do we still need this?

        public void Clock()
        {
            _memoryWriter.Clock();
        }

        public void Reset()
        {
            _memoryWriter.Reset();
        }
    }
}