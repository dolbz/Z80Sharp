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
}