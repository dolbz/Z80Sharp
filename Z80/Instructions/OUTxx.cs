using Z80.AddressingModes;

namespace Z80.Instructions {
    public class OUTxx : IInstruction
    {
        public string Mnemonic => "OUT" + (_increment ? "I" : "D") + (_repeats ? "R" : string.Empty) ;

        public bool IsComplete 
        { 
            get 
            {
                if (_repeats) {
                    if (_repeatCycles.IsComplete) {
                        return true;
                    } else if (_cpu.B == 0 && _outputCycle.IsComplete) {
                        return true;
                    }
                    return false;
                } else {
                    return _outputCycle.IsComplete; 
                }  
            }
        }

        private readonly Z80Cpu _cpu;

        private readonly OutputCycle _outputCycle;
        private readonly InternalCycle _repeatCycles;
        private readonly bool _increment;
        private readonly bool _repeats;

        private int _remainingM1Cycles;
        private IReadAddressedOperand<byte> _sourceReader;

        public OUTxx(Z80Cpu cpu, bool increment, bool repeats) {
            _cpu = cpu;
            _outputCycle = new OutputCycle(cpu);
            _sourceReader = new RegIndirect(cpu, WideRegister.HL).Reader;
            _repeatCycles = new InternalCycle(5);
            _remainingM1Cycles = 1;
            _increment = increment;
            _repeats = repeats;
        }

        public void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                // if (_remainingM1Cycles == -1) {
                //     _inputCycle.Address = (ushort)(_cpu.B << 8 | _cpu.C);
                // }
                if (!_sourceReader.IsComplete) {
                    _sourceReader.Clock();
                    if (_sourceReader.IsComplete) {
                        _outputCycle.Address = (ushort)(--_cpu.B << 8 | _cpu.C);
                        _outputCycle.DataToOutput = _sourceReader.AddressedValue;
                    }
                    return;
                }
                if (!_outputCycle.IsComplete) {
                    _outputCycle.Clock();
                    if (_outputCycle.IsComplete) 
                    {
                        var hlValue = WideRegister.HL.GetValue(_cpu);
                        hlValue = (ushort)(_increment ? hlValue + 1 : hlValue - 1);
                        WideRegister.HL.SetValueOnProcessor(_cpu, hlValue);
                        
                        Z80Flags.Zero_Z.SetOrReset(_cpu, _cpu.B == 0);
                        Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
                    }
                    return;
                }
                if (!_repeatCycles.IsComplete) {
                    _repeatCycles.Clock();
                    if (_repeatCycles.IsComplete) {
                        _cpu.PC -= 2;
                    }
                }
            }
        }

        public void Reset()
        {
            _sourceReader = new RegIndirect(_cpu, WideRegister.HL).Reader;
            _outputCycle.Reset();
            _repeatCycles.Reset();
            _remainingM1Cycles = 1;
        }

        public void StartExecution()
        {
        }
    }
}