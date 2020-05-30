using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class Add : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private readonly bool _withCarry;

        private IReadAddressedOperand<byte> _valueReader;

        public string Mnemonic => _withCarry ? "ADC" : "ADD";

        public bool IsComplete => _valueReader?.IsComplete ?? false;

        public Add(Z80Cpu cpu, IAddressMode<byte> addressMode, bool withCarry = false)
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
                PerformAdd();
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
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, (((_cpu.A & 0x1f) - (_valueReader.AddressedValue & 0xf) - carryIn) & 0x10) == 0);
            _cpu.A = (byte)(0xff & result);
        }
    }
}