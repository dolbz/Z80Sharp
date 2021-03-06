//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 

using Z80.AddressingModes;

namespace Z80.Instructions {
    public class IN : IInstruction
    {
        public string Mnemonic => "IN";

        public bool IsComplete => _inputCycle.IsComplete;

        private readonly Z80Cpu _cpu;
        private readonly Register _destination;
        private readonly IReadAddressedOperand<byte> _source;
        private readonly Register _topHalfOfAddressSource;

        private readonly InputCycle _inputCycle;

        public IN(Z80Cpu cpu, Register destination, IReadAddressedOperand<byte> source, Register topHalfOfAddressSource) {
            _cpu = cpu;
            _destination = destination;
            _source = source;
            _inputCycle = new InputCycle(cpu);
            _topHalfOfAddressSource = topHalfOfAddressSource;
        }

        public void Clock()
        {
            if (!_source.IsComplete) {
                _source.Clock();
                if (_source.IsComplete) {
                    _inputCycle.Address = (ushort)((_topHalfOfAddressSource.GetValue(_cpu) << 8) | _source.AddressedValue);
                }
                return;
            }
            if (!_inputCycle.IsComplete) {
                _inputCycle.Clock();
                if (_inputCycle.IsComplete) {
                    _destination.SetValueOnProcessor(_cpu, _inputCycle.LatchedData);
                    // Flags are only updated for the Z80 added instructions. 
                    // The 8080 compatible instruction does not. Added Z80 instructions use Reg B for top half of address
                    // The 8080 instruction uses the accumulator
                    if (_topHalfOfAddressSource == Register.B) {
                        var input = _inputCycle.LatchedData;
                        Z80Flags.Sign_S.SetOrReset(_cpu, (input & 0x80) == 0x80);
                        Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, input.IsEvenParity());
                        Z80Flags.Zero_Z.SetOrReset(_cpu, (input & 0xff) == 0);
                        Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
                        Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
                    }
                }
            }
        }

        public void Reset()
        {
            _source.Reset();
            _inputCycle.Reset();
        }

        public void StartExecution()
        {
            if (_source.IsComplete) {
                _inputCycle.Address = (ushort)((_topHalfOfAddressSource.GetValue(_cpu) << 8) | _source.AddressedValue);
            }
        }
    }
}