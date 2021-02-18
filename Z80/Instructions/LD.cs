using System;
using Z80.AddressingModes;

namespace Z80.Instructions
{
    public abstract class LD_Generic<T> : IInstruction
    {
        public virtual bool IsComplete => _remainingM1Cycles <= 0 && (_sourceReader?.IsComplete ?? false) && (_destinationWriter?.IsComplete ?? false);
        public virtual string Mnemonic => $"LD {_destinationAddressMode.Description},{_sourceAddressMode.Description}";

        protected Z80Cpu _cpu;
        protected readonly IAddressMode<T> _destinationAddressMode;
        private readonly IAddressMode<T> _sourceAddressMode;

        private IReadAddressedOperand<T> _sourceReader;
        private IWriteAddressedOperand<T> _destinationWriter;

        private readonly int _additionalM1TCycles;
        private readonly bool _setsFlags;
        private int _remainingM1Cycles;

        public LD_Generic(Z80Cpu cpu, IAddressMode<T> destination, IAddressMode<T> source, int additionalM1TCycles, bool setsFlags = false)
        {
            _cpu = cpu;
            _additionalM1TCycles = additionalM1TCycles;
            _remainingM1Cycles = additionalM1TCycles;
            _destinationAddressMode = destination;
            _sourceAddressMode = source;
            _setsFlags = setsFlags;
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
                if (_setsFlags) {
                    Z80Flags.Zero_Z.SetOrReset(_cpu, _cpu.A == 0);
                    Z80Flags.Sign_S.SetOrReset(_cpu, (_cpu.A & 0x8000) == 0x8000);
                    Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, _cpu.IFF2);
                    Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
                    Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
                }
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
        public LD_8Bit(Z80Cpu cpu, IAddressMode<byte> destination, IAddressMode<byte> source, int additionalM1TCycles = 0, bool setsFlags = false)
            : base(cpu, destination, source, additionalM1TCycles, setsFlags)
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