using System;
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class LoadAndXcrement : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly bool _increment;
        private readonly bool _repeats;

        private IReadAddressedOperand<byte> _readCycle;
        private IWriteAddressedOperand<byte> _writeCycle;

        private int _additionalCycles = 2;
        private int _additionalRepeatCycles = 5;

        public string Mnemonic => (_increment ? "LDI" : "LDD") + (_repeats ? "R" : "");

        public bool IsComplete { get; private set; }

        public LoadAndXcrement(Z80Cpu cpu, bool increment, bool withRepeat = false)
        {
            _cpu = cpu;
            _increment = increment;
            _repeats = withRepeat;
        }

        public void Clock()
        {
            // Read from HL pointed address
            if (!_readCycle.IsComplete)
            {
                _readCycle.Clock();
                if (_readCycle.IsComplete)
                {
                    _writeCycle.AddressedValue = _readCycle.AddressedValue;
                }
                return;
            }
            if (!_writeCycle.IsComplete)
            {
                _writeCycle.Clock();
                return;
            }

            var bcValue = WideRegister.BC.GetValue(_cpu);
            if (((!_repeats || bcValue == 1) && --_additionalCycles > 0) || (_repeats && _additionalCycles-- > 0)) // If it's not repeating we need the last cycle of the additional cycles to carry out the instruction
            {
                return;
            }

            if (bcValue != 1 && _repeats && --_additionalRepeatCycles > 0)
            { 
                // Prefix decrement here so we use the last addtional cycle to actually carry out the instruction
                return;
            }

            var deValue = WideRegister.DE.GetValue(_cpu);
            var hlValue = WideRegister.HL.GetValue(_cpu);

            if (_increment)
            {
                deValue++;
                hlValue++;
            }
            else
            {
                deValue--;
                hlValue--;
            }

            WideRegister.DE.SetValueOnProcessor(_cpu, deValue);
            WideRegister.HL.SetValueOnProcessor(_cpu, hlValue);

            WideRegister.BC.SetValueOnProcessor(_cpu, --bcValue);

            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, bcValue == 0);
            if (_repeats && bcValue != 0)
            {
                _cpu.PC -= 2;
            }
            IsComplete = true;
        }

        public void Reset()
        {
            _readCycle = null;
            _writeCycle = null;
            _additionalCycles = 2;
            _additionalRepeatCycles = 5;
            IsComplete = false;
        }

        public void StartExecution()
        {
            _readCycle = new RegIndirect(_cpu, WideRegister.HL).Reader;
            _writeCycle = new RegIndirect(_cpu, WideRegister.DE).Writer;
        }
    }
}