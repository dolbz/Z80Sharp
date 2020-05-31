using Z80.AddressingModes;

namespace Z80.Instructions
{

    public class Increment : IInstruction
    {
        public string Mnemonic => "INC";

        public bool IsComplete => _valueWriter != null && _valueWriter.IsComplete;

        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private IReadAddressedOperand<byte> _valueReader;
        private IWriteAddressedOperand<byte> _valueWriter;

        public Increment(Z80Cpu cpu, IAddressMode<byte> addressMode) 
        {
            _cpu = cpu;
            _addressMode = addressMode;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
    
                if (_addressMode.IsComplete) {
                    _valueReader = _addressMode.Reader;
                    _valueWriter = _addressMode.Writer;
                    if (_valueReader.IsComplete) {
                        PerformIncrement();
                    }
                }
                return;
            }
            if (!_valueReader.IsComplete) {
                _valueReader.Clock();
                if (_valueReader.IsComplete) {
                    PerformIncrement();
                }
                return;
            }
            if (!_valueWriter.IsComplete) {
                _valueWriter.Clock();
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _valueReader = null;
            _valueWriter = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete)
            {
                _valueReader = _addressMode.Reader;
                _valueWriter = _addressMode.Writer;
                if (_valueReader.IsComplete) {
                    PerformIncrement();
                }
            }
        }

        private void PerformIncrement() 
        {
            var result = _valueReader.AddressedValue + 1;

            _valueWriter.AddressedValue = (byte)result;
            
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x80) == 0x80);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, result > 127);
            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xff) == 0);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, result == 0x10);
        }
    }
}