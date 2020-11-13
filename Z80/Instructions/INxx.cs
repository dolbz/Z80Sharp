using System.Collections.Specialized;
using System.Text;
using System;
using Z80.AddressingModes;

namespace Z80.Instructions {
    public class INxx : IInstruction
    {
        public string Mnemonic => "IN" + (_increment ? "I" : "D") + (_repeats ? "R" : string.Empty) ;

        public bool IsComplete 
        { 
            get 
            {
                if (_repeats) {
                    if (_repeatCycles.IsComplete) {
                        return true;
                    } else if (_cpu.B == 0 && (_destinationWriter?.IsComplete ?? false)) {
                        return true;
                    }
                    return false;
                } else {
                    return _destinationWriter?.IsComplete ?? false; 
                }  
            }
        }

        private readonly Z80Cpu _cpu;

        private readonly RegIndirect _destination;

        private readonly InputCycle _inputCycle;
        private readonly InternalCycle _repeatCycles;
        private readonly bool _increment;
        private readonly bool _repeats;

        private  IWriteAddressedOperand<byte> _destinationWriter;
        private int _remainingM1Cycles;

        public INxx(Z80Cpu cpu, bool increment, bool repeats) {
            _cpu = cpu;
            _inputCycle = new InputCycle(cpu);
            _destination = new RegIndirect(cpu, WideRegister.HL);
            _repeatCycles = new InternalCycle(5);
            _remainingM1Cycles = 1;
            _increment = increment;
            _repeats = repeats;
        }

        public void Clock()
        {
            if (_remainingM1Cycles-- <= 0)
            {
                if (_remainingM1Cycles == -1) {
                    _inputCycle.Address = (ushort)(_cpu.B << 8 | _cpu.C);
                }
                if (!_inputCycle.IsComplete) {
                    _inputCycle.Clock();
                    if (_inputCycle.IsComplete) {
                        _destinationWriter = _destination.Writer;
                        _destinationWriter.AddressedValue = _inputCycle.LatchedData;
                    }
                    return;
                }
                if (!_destinationWriter.IsComplete)
                {
                    _destinationWriter.Clock();
                    if (_destinationWriter.IsComplete) {
                        var hlValue = WideRegister.HL.GetValue(_cpu);
                        hlValue = (ushort)(_increment ? hlValue + 1 : hlValue - 1);
                        WideRegister.HL.SetValueOnProcessor(_cpu, hlValue);
                        Register.B.SetValueOnProcessor(_cpu, (byte)(_cpu.B - 1));
                        var input = _inputCycle.LatchedData;
                        
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
            _inputCycle.Reset();
            _destinationWriter = null;
            _repeatCycles.Reset();
            _remainingM1Cycles = 1;
        }

        public void StartExecution()
        {
        }
    }
}