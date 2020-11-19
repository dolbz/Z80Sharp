using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal abstract class LD_Generic<T> : IInstruction
    {
        public virtual bool IsComplete => _remainingM1Cycles <= 0 && (_sourceReader?.IsComplete ?? false) && (_destinationWriter?.IsComplete ?? false);
        public virtual string Mnemonic => $"LD {_destinationAddressMode.Description},{_sourceAddressMode.Description}";

        protected Z80Cpu _cpu;
        private readonly IAddressMode<T> _destinationAddressMode;
        private readonly IAddressMode<T> _sourceAddressMode;

        private IReadAddressedOperand<T> _sourceReader;
        private IWriteAddressedOperand<T> _destinationWriter;

        private readonly int _additionalM1TCycles;
        private int _remainingM1Cycles;

        public LD_Generic(Z80Cpu cpu, IAddressMode<T> destination, IAddressMode<T> source, int additionalM1TCycles)
        {
            _cpu = cpu;
            _additionalM1TCycles = additionalM1TCycles;
            _remainingM1Cycles = additionalM1TCycles;
            _destinationAddressMode = destination;
            _sourceAddressMode = source;
        }

        public virtual void StartExecution()
        {
            if (_sourceAddressMode.IsComplete) 
            {
                _sourceReader = _sourceAddressMode.Reader;
            }
            if (_destinationAddressMode.IsComplete) 
            {
                _destinationWriter = _destinationAddressMode.Writer;
            }

            if ((_sourceReader?.IsComplete ?? false) && (_destinationAddressMode?.IsComplete ?? false))
            {
                _destinationWriter.AddressedValue = _sourceAddressMode.Reader.AddressedValue;
            }
        }

        public virtual void Reset()
        {
            _sourceAddressMode.Reset();
            _destinationAddressMode.Reset();
            _sourceReader = null;
            _destinationWriter = null;
            _remainingM1Cycles = _additionalM1TCycles;
        }

        public virtual void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                // Destination is checked first as its operand comes first if there are 
                // additional bytes to the instruction.
                if (!_destinationAddressMode.IsComplete)
                {
                    _destinationAddressMode.Clock();
                    if (_destinationAddressMode.IsComplete) {
                        _destinationWriter = _destinationAddressMode.Writer;
                        if (_sourceReader?.IsComplete ?? false) {
                            _destinationWriter.AddressedValue = _sourceReader.AddressedValue;
                        }
                    }
                    return;
                }
                if (!_sourceAddressMode.IsComplete)
                {
                    _sourceAddressMode.Clock();

                    if (_sourceAddressMode.IsComplete)
                    {
                        _sourceReader = _sourceAddressMode.Reader;
                        if (_sourceReader.IsComplete) {
                            _destinationWriter.AddressedValue = _sourceReader.AddressedValue;
                        }
                    }
                    return;
                }
                if (!_sourceReader.IsComplete) {
                    _sourceReader.Clock();
                    if (_sourceReader.IsComplete) {
                        _destinationWriter.AddressedValue = _sourceReader.AddressedValue;
                    }
                    return;
                }
                if (!_destinationWriter.IsComplete)
                {
                    _destinationWriter.Clock();
                }
            }
        }
    }

    internal class LD_8Bit : LD_Generic<byte>
    {
        public LD_8Bit(Z80Cpu cpu, IAddressMode<byte> destination, IAddressMode<byte> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }

    internal class LD_16Bit : LD_Generic<ushort>
    {
        public LD_16Bit(Z80Cpu cpu, IAddressMode<ushort> destination, IAddressMode<ushort> source, int additionalM1TCycles = 0)
            : base(cpu, destination, source, additionalM1TCycles)
        {
        }
    }
}