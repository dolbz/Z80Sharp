//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class CompareAndXcrement : IInstruction
    {
        private readonly bool _increment;
        private readonly bool _repeats;
        private readonly Z80Cpu _cpu;
        private IReadAddressedOperand<byte> _readCycle;

        private int _additionalCycles = 5;
        private int _additionalRepeatCycles = 5;

        public string Mnemonic => (_increment ? "CPI" : "CPD") + (_repeats ? "R" : "");
        public bool IsComplete { get; private set; }

        public CompareAndXcrement(Z80Cpu cpu, bool increment, bool withRepeat = false)
        {
            _cpu = cpu;
            _increment = increment;
            _repeats = withRepeat;
        }

        public void Clock()
        {
            if (!_readCycle.IsComplete)
            {
                _readCycle.Clock();
                return;
            }

            var bcValue = WideRegister.BC.GetValue(_cpu);
            if (((!_repeats || bcValue == 1) && --_additionalCycles > 0) || (_repeats && _additionalCycles-- > 0)) // If it's not repeating we need the last cycle of the additional cycles to carry out the instruction
            {
                return;
            }

            if (bcValue != 1 && _repeats && --_additionalRepeatCycles > 0)
            { // Prefix decrement here so we use the last addtional cycle to actually carry out the instruction
                return;
            }

            // Set flags as appropriate
            var difference = _cpu.A - _readCycle.AddressedValue;
            Z80Flags.Sign_S.SetOrReset(_cpu, difference < 0);
            Z80Flags.Zero_Z.SetOrReset(_cpu, difference == 0);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, (_cpu.A & 0xf) < (_readCycle.AddressedValue & 0xf));
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, bcValue != 1);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);

            // Update registers
            var hlValue = WideRegister.HL.GetValue(_cpu);

            if (_increment)
            {
                hlValue++;
            }
            else
            {
                hlValue--;
            }

            WideRegister.HL.SetValueOnProcessor(_cpu, hlValue);

            WideRegister.BC.SetValueOnProcessor(_cpu, --bcValue);

            if (_repeats && bcValue != 0 && difference != 0)
            {
                _cpu.PC -= 2;
            }
            IsComplete = true;
        }

        public void Reset()
        {
            _readCycle = null;
            _additionalCycles = 5;
            _additionalRepeatCycles = 5;
            IsComplete = false;
        }

        public void StartExecution()
        {
            _readCycle = new RegIndirect(_cpu, WideRegister.HL).Reader;
        }
    }
}