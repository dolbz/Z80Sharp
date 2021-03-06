//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;

namespace Z80.AddressingModes {
    public class RelativeAddressMode : IAddressMode<ushort>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemoryByteReader _offsetReader;
    
        public IReadAddressedOperand<ushort> Reader => new RelativeReader(_cpu, (sbyte)_offsetReader.AddressedValue);

        public IWriteAddressedOperand<ushort> Writer => throw new InvalidOperationException("Cannot write with the relative addressing mode");

        public bool IsComplete => _offsetReader.IsComplete;

        public string Description => "PC+e";

        public RelativeAddressMode(Z80Cpu cpu) {
            _cpu = cpu;
            _offsetReader = new MemoryByteReader(cpu);
        }

        public void Clock()
        {
            if (!_offsetReader.IsComplete) {
                _offsetReader.Clock();
                return;
            }
        }

        public void Reset()
        {
            _offsetReader.Reset();
        }
    }
}