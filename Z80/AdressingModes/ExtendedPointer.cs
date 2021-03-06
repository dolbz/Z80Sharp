//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.AddressingModes
{
    public struct ExtendedPointer : IAddressMode<byte>
    {
        private readonly Z80Cpu _cpu;
        private readonly MemoryShortReader _extendedOperand;

        public bool IsComplete => _extendedOperand.IsComplete;

        public IReadAddressedOperand<byte> Reader => new MemoryByteReader(_cpu, _extendedOperand.AddressedValue);

        public IWriteAddressedOperand<byte> Writer => new MemoryByteWriter(_cpu, _extendedOperand.AddressedValue);

        public string Description => "(nn)";

        public ExtendedPointer(Z80Cpu cpu)
        {
            _cpu = cpu;
            _extendedOperand = new MemoryShortReader(cpu);
        }

        public void Clock()
        {
            if (!_extendedOperand.IsComplete)
            {
                _extendedOperand.Clock();
            }
        }

        public void Reset()
        {
            _extendedOperand.Reset();
        }
    }
}