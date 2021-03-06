//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Reflection.Emit;
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class BitTest : IInstruction
    {
        private Z80Cpu _cpu;
        private IAddressMode<byte> _addressMode;
        private int _bitNumber;

        private IReadAddressedOperand<byte> _reader;

        public string Mnemonic => "BIT";

        public bool IsComplete => _reader != null && _reader.IsComplete;

        public BitTest(Z80Cpu cpu, IAddressMode<byte> addressMode, int bitNumber) {
            _cpu = cpu;
            _addressMode = addressMode;
            _bitNumber = bitNumber;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
                if (_addressMode.IsComplete) {
                    _reader = _addressMode.Reader;
                    if (_reader.IsComplete) {
                        PerformBitTest();
                    }
                }
                return;
            }
            if (!_reader.IsComplete) {
                _reader.Clock();
                if (_reader.IsComplete) {
                    PerformBitTest();
                }
            }
        }

        private void PerformBitTest() {
            var readValue = _reader.AddressedValue;
            var bitZero = ((readValue >> _bitNumber) & 1) == 0;

            Z80Flags.Zero_Z.SetOrReset(_cpu, bitZero);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, true);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
        }

        public void Reset()
        {
            _addressMode.Reset();
            _reader = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete) {
                _reader = _addressMode.Reader;
                if (_reader.IsComplete) {
                    PerformBitTest();
                }
            }
        }
    }
}