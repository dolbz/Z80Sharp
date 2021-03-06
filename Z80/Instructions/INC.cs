//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
﻿using Z80.AddressingModes;

namespace Z80.Instructions
{

    public class Increment_8bit : IInstruction
    {
        public string Mnemonic => $"INC {_addressMode.Description}";

        public bool IsComplete => _valueWriter != null && _valueWriter.IsComplete;

        private readonly Z80Cpu _cpu;
        private readonly IAddressMode<byte> _addressMode;
        private IReadAddressedOperand<byte> _valueReader;
        private IWriteAddressedOperand<byte> _valueWriter;

        public Increment_8bit(Z80Cpu cpu, IAddressMode<byte> addressMode) 
        {
            _cpu = cpu;
            _addressMode = addressMode;
        }

        public void Clock()
        {
            if (!_addressMode.IsComplete) {
                _addressMode.Clock();
    
                if (_addressMode.IsComplete) {
                    _valueReader = _addressMode.Reader;
                    _valueWriter = _addressMode.Writer;
                    if (_valueReader.IsComplete) {
                        PerformIncrement();
                    }
                }
                return;
            }
            if (!_valueReader.IsComplete) {
                _valueReader.Clock();
                if (_valueReader.IsComplete) {
                    PerformIncrement();
                }
                return;
            }
            if (!_valueWriter.IsComplete) {
                _valueWriter.Clock();
            }
        }

        public void Reset()
        {
            _addressMode.Reset();
            _valueReader = null;
            _valueWriter = null;
        }

        public void StartExecution()
        {
            if (_addressMode.IsComplete)
            {
                _valueReader = _addressMode.Reader;
                _valueWriter = _addressMode.Writer;
                if (_valueReader.IsComplete) {
                    PerformIncrement();
                }
            }
        }

        private void PerformIncrement() 
        {
            var result = _valueReader.AddressedValue + 1;

            _valueWriter.AddressedValue = (byte)result;
            
            Z80Flags.Sign_S.SetOrReset(_cpu, (result & 0x80) == 0x80);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, result == 128);
            Z80Flags.Zero_Z.SetOrReset(_cpu, (result & 0xff) == 0);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, (result & 0xf) == 0);
        }
    }

    public class Increment_16bit : IInstruction
    {
        public string Mnemonic => "INC";

        public bool IsComplete => _remainingM1Cycles <= 0;

        private readonly Z80Cpu _cpu;
        private readonly RegAddrMode16Bit _addressMode;
        
        private int _remainingM1Cycles;

        public Increment_16bit(Z80Cpu cpu, RegAddrMode16Bit addressMode) 
        {
            _cpu = cpu;
            _addressMode = addressMode;
            _remainingM1Cycles = 2;
        }

        public void Clock()
        {
            if (--_remainingM1Cycles <= 0)
            {
                _addressMode.Writer.AddressedValue = (ushort)(_addressMode.Reader.AddressedValue + 1);
            }
        }

        public void Reset()
        {
            _remainingM1Cycles = 2;
        }

        public void StartExecution()
        {
        }
    }
}