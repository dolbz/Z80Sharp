using System;
namespace Z80.AddressingModes
{
    public struct RegIndirect : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly WideRegister _register;
        private bool _additonalCycleOnReader;

        public bool IsComplete => true;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu, _register.GetValue(_cpu), _additonalCycleOnReader);

        public IWriteAddressedOperand<byte> Writer => new MemoryByteWriter(_cpu, _register.GetValue(_cpu));

        public string Description => $"({_register.ToString()})";

        public RegIndirect(Z80Cpu cpu, WideRegister register, bool additionalCycleOnRead = false)
        {
            _cpu = cpu;
            _register = register;
            _additonalCycleOnReader = additionalCycleOnRead;
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