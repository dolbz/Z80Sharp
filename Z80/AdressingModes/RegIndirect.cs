using System;
namespace Z80.AddressingModes
{
    public struct RegIndirect : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly WideRegister _register;
        private readonly InternalCycle _internalCycle;

        public bool IsComplete => _internalCycle.IsComplete;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu, _register.GetValue(_cpu));

        public IWriteAddressedOperand<byte> Writer => new MemoryByteWriter(_cpu, _register.GetValue(_cpu));

        public RegIndirect(Z80Cpu cpu, WideRegister register, int additionalCycles = 0)
        {
            _cpu = cpu;
            _register = register;
            _internalCycle = new InternalCycle(additionalCycles);
        }

        public void Clock()
        {
            _internalCycle.Clock();
        }

        public void Reset()
        {
            _internalCycle.Reset();
        }
    }
}