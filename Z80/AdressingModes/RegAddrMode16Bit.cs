
using System;

namespace Z80.AddressingModes
{
    public struct RegAddrMode16Bit : IWriteAddressedOperand<ushort>, IReadAddressedOperand<ushort>, IAddressMode<ushort>
    {
        public readonly WideRegister _register;
        public readonly Z80Cpu _processor;
        public ushort AddressedValue
        {
            get => _register.GetValue(_processor);
            set => _register.SetValueOnProcessor(_processor, value);
        }

        public bool IsComplete => true;
        public bool WriteReady => true;

        public IReadAddressedOperand<ushort> Reader => this;

        public IWriteAddressedOperand<ushort> Writer => this;

        public RegAddrMode16Bit(Z80Cpu processor, WideRegister register)
        {
            _processor = processor;
            _register = register;
        }

        public void Reset()
        {
            // Nothing to do
        }

        public void Clock()
        {
            throw new InvalidOperationException("This isn't expected to be called as IsComplete is true");
        }
    }
}