using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class SubtractOrCompare : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private IReadAddressedOperand<byte> _readOperand;
        private readonly bool _withCarry;

        private readonly bool _updateAccumulator;

        public string Mnemonic
        {
            get
            {
                if (_updateAccumulator)
                {
                    return _withCarry ? "SBC" : "SUB";
                }
                else
                {
                    return "CP";
                }
            }
        }

        public bool IsComplete => _readOperand?.IsComplete ?? false;

        public SubtractOrCompare(Z80Cpu cpu, IAddressMode<byte> addressMode, bool withCarry = false, bool updateAccumulator = true)
        {
            _cpu = cpu;
            _addressMode = addressMode;
            _withCarry = withCarry;
            _updateAccumulator = updateAccumulator;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete)
            {
                _addressMode.Clock();
                if (_addressMode.IsComplete)
                {
                    _readOperand = _addressMode.Reader;
                    if (_readOperand.IsComplete) {
                        PerformSubtraction();
                    }
                }
                return;
            }
            if (!_readOperand.IsComplete) {
                _readOperand.Clock();
                if (_readOperand.IsComplete) {
                    PerformSubtraction();
                }
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _readOperand = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete)
            {
                _readOperand = _addressMode.Reader;

                if (_readOperand.IsComplete) {
                    PerformSubtraction();
                }
            }
        }

        private void PerformSubtraction()
        {
            var carryIn = 0;
            if (_withCarry && _cpu.Flags.HasFlag(Z80Flags.Carry_C))
            {
                carryIn = 1;
            }
            var result = _cpu.A - _readOperand.AddressedValue - carryIn;
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x80) == 0x80);
            Z80Flags.Carry_C.SetOrReset(_cpu, result < 0);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, (_cpu.A >= 0x80 && result < 0x80) || result < -128);
            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xff) == 0);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, (((_cpu.A & 0xf) - (_readOperand.AddressedValue & 0xf) - carryIn) & 0x10) == 0x10);
            if (_updateAccumulator)
            {
                _cpu.A = (byte)(0xff & result);
            }
        }
    }

    internal class SubtractWithCarry_16bit : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly RegAddrMode16Bit _destinationAddressMode;
        private readonly RegAddrMode16Bit _sourceAddressMode;
        private InternalCycle _internalCycle;

        public string Mnemonic => "SBC";

        public bool IsComplete => _internalCycle.IsComplete;

        public SubtractWithCarry_16bit(Z80Cpu cpu, RegAddrMode16Bit destinationAddressMode, RegAddrMode16Bit sourceAddressMode)
        {
            _cpu = cpu;
            _destinationAddressMode = destinationAddressMode;
            _sourceAddressMode = sourceAddressMode;
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
            if (_cpu.Flags.HasFlag(Z80Flags.Carry_C))
            {
                carryIn = 1;
            }
            var destOriginalValue = _destinationAddressMode.Reader.AddressedValue;
            var sourceOriginalValue = _sourceAddressMode.Reader.AddressedValue;

            var result = destOriginalValue - sourceOriginalValue - carryIn;

            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xffff) == 0);
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x8000) == 0x8000);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, (destOriginalValue >= 0x8000 && result < 0x8000) || result < -32768);

            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            Z80Flags.Carry_C.SetOrReset(_cpu, destOriginalValue < sourceOriginalValue + carryIn);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, ((destOriginalValue & 0xfff) - (sourceOriginalValue & 0xfff) - carryIn < 0));

            _destinationAddressMode.Writer.AddressedValue = (ushort)result;
        }
    }
}