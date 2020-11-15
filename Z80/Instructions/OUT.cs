using System.Diagnostics;

using Z80.AddressingModes;

namespace Z80.Instructions {
    public class OUT : IInstruction
    {
        public string Mnemonic => "OUT";

        public bool IsComplete => _outputCycle.IsComplete;

        private readonly Z80Cpu _cpu;
        private readonly IReadAddressedOperand<byte> _destination;
        private readonly Register _source;
        private readonly Register _topHalfOfAddressSource;

        private readonly OutputCycle _outputCycle;

        public OUT(Z80Cpu cpu, IReadAddressedOperand<byte> destination, Register source, Register topHalfOfAddressSource) {
            _cpu = cpu;
            _destination = destination;
            _source = source;
            _outputCycle = new OutputCycle(cpu);
            _topHalfOfAddressSource = topHalfOfAddressSource;
        }

        public void Clock()
        {
            if (!_destination.IsComplete) {
                _destination.Clock();
                if (_destination.IsComplete) {
                    _outputCycle.Address = (ushort)((_topHalfOfAddressSource.GetValue(_cpu) << 8) | _destination.AddressedValue);
                    _outputCycle.DataToOutput = _source.GetValue(_cpu);
                }
                return;
            }
            if (!_outputCycle.IsComplete) {
                _outputCycle.Clock();
            }
        }

        public void Reset()
        {
            _destination.Reset();
            _outputCycle.Reset();
        }

        public void StartExecution()
        {
            if (_destination.IsComplete) {
                _outputCycle.Address = (ushort)((_topHalfOfAddressSource.GetValue(_cpu) << 8) | _destination.AddressedValue);
                _outputCycle.DataToOutput = _source.GetValue(_cpu);
            }
        }
    }
}