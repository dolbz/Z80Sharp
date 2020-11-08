using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class Add_8bit : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private readonly bool _withCarry;

        private IReadAddressedOperand<byte> _valueReader;

        public string Mnemonic => _withCarry ? "ADC" : "ADD";

        public bool IsComplete => _valueReader?.IsComplete ?? false;

        public Add_8bit(Z80Cpu cpu, IAddressMode<byte> addressMode, bool withCarry = false)
        {
            _cpu = cpu;
            _addressMode = addressMode;
            _withCarry = withCarry;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete)
            {
                _addressMode.Clock();
                if (_addressMode.IsComplete)
                {
                    _valueReader = _addressMode.Reader;
                    if (_valueReader.IsComplete) 
                    {
                        PerformAdd();
                    }
                }
                return;
            }
            if (!_valueReader.IsComplete) {
                _valueReader.Clock();
                if (_valueReader.IsComplete) {
                    PerformAdd();
                }
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _valueReader = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete)
            {
                _valueReader = _addressMode.Reader;

                if (_valueReader.IsComplete) 
                {
                    PerformAdd();
                }
            }
        }

        private void PerformAdd()
        {
            var carryIn = 0;
            if (_withCarry && _cpu.Flags.HasFlag(Z80Flags.Carry_C))
            {
                carryIn = 1;
            }

            var result = _cpu.A + _valueReader.AddressedValue + carryIn;
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x80) == 0x80);
            Z80Flags.Carry_C.SetOrReset(_cpu, result > 0xff);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, result > 127);
            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xff) == 0);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, ((_cpu.A & 0xf) + (_valueReader.AddressedValue & 0xf) + carryIn >= 0x10));
            _cpu.A = (byte)(0xff & result);
        }
    }

    internal class Add_16bit : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly RegAddrMode16Bit _destinationAddressMode;
        private readonly RegAddrMode16Bit _sourceAddressMode;
        private readonly bool _withCarry;
        private InternalCycle _internalCycle;

        public string Mnemonic => _withCarry ? "ADC" : "ADD";

        public bool IsComplete => _internalCycle.IsComplete;

        public Add_16bit(Z80Cpu cpu, RegAddrMode16Bit destinationAddressMode, RegAddrMode16Bit sourceAddressMode, bool withCarry = false)
        {
            _cpu = cpu;
            _destinationAddressMode = destinationAddressMode;
            _sourceAddressMode = sourceAddressMode;
            _withCarry = withCarry;
            _internalCycle = new InternalCycle(7);
        }

        public void Clock()
        {
            // Destination and source for 16-bit add instructions are always 16-bit register pairs
            // This means we don't need to check address mode completion or reader/writer completion
            // as it's known they are always complete and can be used immediately. We just need to wait
            // for the internal cycle to complete

            if (!_internalCycle.IsComplete) {
                _internalCycle.Clock();

                if (_internalCycle.IsComplete) {
                    PerformAdd();
                }
            }
        }

        public void Reset()
        {
            // There is no need to call reset on the source/destination as register address modes 
            // don't have any behaviour on reset
            _internalCycle.Reset();
        }

        public void StartExecution()
        {
            // Never anything to do for 16-bit additions immediately
        }

        private void PerformAdd()
        {
            var carryIn = 0;
            if (_withCarry && _cpu.Flags.HasFlag(Z80Flags.Carry_C))
            {
                carryIn = 1;
            }
            var destOriginalValue = _destinationAddressMode.Reader.AddressedValue;
            var sourceOriginalValue = _sourceAddressMode.Reader.AddressedValue;

            var result = destOriginalValue + sourceOriginalValue + carryIn;

            if (_withCarry) {
                // Instructions with carry affect the Sign and Zero flags
                Z80Flags.Zero_Z.SetOrReset(_cpu, result == 0);
                Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x8000) == 0x8000);
                Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, result > 0x7fff);
            }

            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.Carry_C.SetOrReset(_cpu, result > 0xffff);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, ((destOriginalValue & 0xfff) + (sourceOriginalValue & 0xfff) + carryIn >= 0x1000));

            _destinationAddressMode.Writer.AddressedValue = (ushort)result;
        }
    }
}