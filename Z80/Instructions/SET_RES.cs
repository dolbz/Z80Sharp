using System.Reflection.Emit;
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class SetOrResetBit : IInstruction
    {
        private Z80Cpu _cpu;
        private IAddressMode<byte> _addressMode;
        private int _bitNumber;
        private bool _isSet;

        private IReadAddressedOperand<byte> _reader;
        private IWriteAddressedOperand<byte> _writer;

        public string Mnemonic => "RES";

        public bool IsComplete => _writer != null && _writer.IsComplete;

        public SetOrResetBit(Z80Cpu cpu, IAddressMode<byte> addressMode, int bitNumber, bool isSet) {
            _cpu = cpu;
            _addressMode = addressMode;
            _bitNumber = bitNumber;
            _isSet = isSet;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
                if (_addressMode.IsComplete) {
                    _reader = _addressMode.Reader;
                    if (_reader.IsComplete) {
                        ManipulateBit();
                    }
                }
                return;
            }
            if (!_reader.IsComplete) {
                _reader.Clock();
                if (_reader.IsComplete) {
                    ManipulateBit();
                }
                return;
            }
            if (!_writer.IsComplete) {
                _writer.Clock();
            }
        }

        private void ManipulateBit() {
            var bitMask = 1 << _bitNumber;
            if (_isSet) {
                _writer.AddressedValue = (byte)(_reader.AddressedValue | bitMask);
            } else {
                bitMask = (byte)~bitMask;
                _writer.AddressedValue = (byte)(_reader.AddressedValue & bitMask);
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _reader = null;
            _writer = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete) {
                _reader = _addressMode.Reader;
                _writer = _addressMode.Writer;
                if (_reader.IsComplete) {
                    ManipulateBit();
                }
            }
        }
    }
}