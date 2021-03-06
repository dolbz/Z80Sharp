//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class AND : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private IReadAddressedOperand<byte> _readOperand;

        public string Mnemonic => "AND";

        public bool IsComplete => _readOperand?.IsComplete ?? false;

        public AND(Z80Cpu cpu, IAddressMode<byte> addressMode)
        {
            _cpu = cpu;
            _addressMode = addressMode;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
                if (_addressMode.IsComplete) {
                    _readOperand = _addressMode.Reader;
                    if (_readOperand.IsComplete) {
                        PerformAND();
                    }
                }
                return;
            }
            if (!_readOperand.IsComplete)
            {
                _readOperand.Clock();
                if (_readOperand.IsComplete)
                {
                    PerformAND();
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
                    PerformAND();
                }
            }
        }

        private void PerformAND()
        {
            var result = (byte)(_cpu.A & _readOperand.AddressedValue);
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x80) == 0x80);
            Z80Flags.Carry_C.SetOrReset(_cpu, false);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, result.IsEvenParity());
            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xff) == 0);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, true);
            _cpu.A = result;
        }
    }
}