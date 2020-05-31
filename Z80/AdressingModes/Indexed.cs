
using System;

namespace Z80.AddressingModes
{
    public class Indexed : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemReadCycle _offsetReadCycle;
        private readonly InternalCycle _internalCycle;
        private readonly WideRegister _register;
        private readonly bool _additionalCycleOnRead;

        public bool WriteReady => _offsetReadCycle.IsComplete;

        public bool IsComplete => _internalCycle.IsComplete;

        public IReadAddressedOperand<byte> Reader {
            get 
            {
                var reader = new MemoryByteReader(_cpu, GetIndexedAddress(), _additionalCycleOnRead);
                return reader;
            }
        }

        public IWriteAddressedOperand<byte> Writer {
            get 
            {
                var writer = new MemoryByteWriter(_cpu, GetIndexedAddress());
                return writer;
            }
        }

        private ushort GetIndexedAddress() {
            return (ushort)((_register == WideRegister.IX ? _cpu.IX : _cpu.IY) + (sbyte)_offsetReadCycle.LatchedData);
        }

        public Indexed(Z80Cpu cpu, WideRegister register, int internalCycleLength = 5, bool additionalCycleOnRead = false)
        {
            if (register != WideRegister.IX && register != WideRegister.IY)
            {
                throw new InvalidOperationException("Invald index register specified");
            }
            _register = register;
            _cpu = cpu;
            _offsetReadCycle = new MemReadCycle(cpu);
            _internalCycle = new InternalCycle(internalCycleLength);
            _additionalCycleOnRead = additionalCycleOnRead;
        }

        public void Reset()
        {
            _offsetReadCycle.Reset();
            _internalCycle.Reset();
        }

        public void Clock()
        {
            if (!_offsetReadCycle.IsComplete)
            {
                _offsetReadCycle.Clock();
                return;
            }

            if (!_internalCycle.IsComplete)
            {
                _internalCycle.Clock();
            }
        }
    }
}