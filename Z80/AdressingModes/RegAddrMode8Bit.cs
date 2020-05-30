
using System;

namespace Z80.AddressingModes
{
    public struct RegAddrMode8Bit : IWriteAddressedOperand<byte>, IReadAddressedOperand<byte>, IAddressMode<byte>
    {
        public readonly Register _register;
        public readonly Z80Cpu _processor;
        public byte AddressedValue
        {
            get => _register.GetValue(_processor);
            set => _register.SetValueOnProcessor(_processor, value);
        }

        public bool IsComplete => true;
        public bool WriteReady => true;

        public IReadAddressedOperand<byte> Reader => this;

        public IWriteAddressedOperand<byte> Writer => this;

        public RegAddrMode8Bit(Z80Cpu processor, Register register)
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