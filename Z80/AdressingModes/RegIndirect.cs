using System;
namespace Z80.AddressingModes
{
    public struct RegIndirect : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly WideRegister _register;

        public bool IsComplete => true;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu, _register.GetValue(_cpu));

        public IWriteAddressedOperand<byte> Writer => new MemoryByteWriter(_cpu, _register.GetValue(_cpu));

        public RegIndirect(Z80Cpu cpu, WideRegister register)
        {
            _cpu = cpu;
            _register = register;
        }

        public void Clock()
        {
            throw new InvalidOperationException();
        }

        public void Reset()
        {
        }
    }
}