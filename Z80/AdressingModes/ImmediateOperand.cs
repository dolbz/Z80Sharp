//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 

using System;

namespace Z80.AddressingModes
{
    public struct ImmediateOperand : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        public bool IsComplete => true;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu);

        public IWriteAddressedOperand<byte> Writer => throw new InvalidOperationException("Cannot write with the immediate addressing mode");

        public string Description => "n";

        public ImmediateOperand(Z80Cpu cpu)
        {
            _cpu = cpu;
        }

        public void Clock()
        {
            throw new InvalidOperationException("This address mode should not be clocked");
        }

        public void Reset()
        {
        }
    }
}