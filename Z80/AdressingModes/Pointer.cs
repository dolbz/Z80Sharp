namespace Z80.AddressingModes
{
    public class Pointer : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _pointerAddressMode;
        private IReadAddressedOperand<byte> _reader;

        public bool IsComplete => _pointerAddressMode.IsComplete;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu, _reader.AddressedValue);

        public IWriteAddressedOperand<byte> Writer => new MemoryByteWriter(_cpu, _reader.AddressedValue);

        public Pointer(Z80Cpu cpu, IAddressMode<byte> pointerAddressMode)
        {
            _cpu = cpu;
            _pointerAddressMode = pointerAddressMode;
        }

        public void Clock()
        {
            if (!_pointerAddressMode.IsComplete)
            {
                _pointerAddressMode.Clock();
                if (_pointerAddressMode.IsComplete) 
                {
                    _reader = _pointerAddressMode.Reader;
                }
            }
        }

        public void Reset()
        {
            _pointerAddressMode.Reset();
            _reader = null;
        }
    }
}