using System.Diagnostics;
namespace Z80 {
    internal interface IInstruction : IClockable {        
        string Mnemonic { get; }

        // Called on the final clock cycle of the instruction fetch (M1) cycle.
        // If the instruction can be completed without additional cycles it
        // performs the behaviour and IsComplete returns true so that the next
        // fetch cycle can be started immediately. Otherwise the IInstruction object
        // receives future Clock() invocations until IsComplete returns true 
        void StartExecution();
    }

    internal class NOP : IInstruction {
        
        public string Mnemonic => "NOP";

        public bool IsComplete => true;

        public void StartExecution() {
            // No operation
        }
        public void Clock() {
            // Do no operation
        }

        public void Reset() {
            // Nothing to do
        }
    }

    internal class LD_8Bit : IInstruction
    {   
        public bool IsComplete => _destination.IsComplete && _source.IsComplete;
        public string Mnemonic { get; }

        private Z80Cpu _cpu;
        private IWriteAddressedOperand<byte> _destination;
        private IReadAddressedOperand<byte> _source;

        private readonly int _additionalM1TCycles;
        private int _remainingM1Cycles;

        public LD_8Bit(Z80Cpu cpu, IWriteAddressedOperand<byte> destination, IReadAddressedOperand<byte> source, int additionalM1TCycles = 0) {
            _cpu = cpu;
            Mnemonic = "LD";
            _additionalM1TCycles = additionalM1TCycles;
            _destination = destination;
            _source = source;
        }

        public void StartExecution() {
            if (_remainingM1Cycles == 0 && _destination.IsComplete && _source.IsComplete) {
                _destination.AddressedValue = _source.AddressedValue;
            }
        }

        public void Reset() {
            _source.Reset();
            _destination.Reset();
            _remainingM1Cycles = _additionalM1TCycles;
        }

        public void Clock() {
            if (--_remainingM1Cycles <= 0) {
                // Destination is checked first as its operand comes first if there are 
                // additional bytes to the instruction.
                if (!_destination.WriteReady) {
                    _destination.Clock();
                    return;
                }
                if (!_source.IsComplete) {
                    _source.Clock();

                    if (_source.IsComplete) {
                        _destination.AddressedValue = _source.AddressedValue;
                    }
                }

                if (!_destination.IsComplete) {
                    _destination.Clock();
                }
            }
        }
    }
}