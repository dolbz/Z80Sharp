//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
namespace Z80.AddressingModes
{
    public class RegIndirectWide : IAddressMode<ushort>
    {
        private readonly Z80Cpu _cpu;
        private readonly WideRegister _register;
        private readonly bool _decreasingAddress;

        public bool IsComplete => true;

        public IReadAddressedOperand<ushort> Reader => new MemoryShortReader(_cpu, _register.GetValue(_cpu));

        public IWriteAddressedOperand<ushort> Writer => new MemoryShortWriter(_cpu, _register.GetValue(_cpu), _decreasingAddress);

        public string Description => $"({_register.ToString()})";

        public RegIndirectWide(Z80Cpu cpu, WideRegister register, bool decreasingAddress)
        {
            _decreasingAddress = decreasingAddress;
            _cpu = cpu;
            _register = register;
        }

        public void Clock()
        {
            throw new InvalidOperationException("This address mode shouldn't be clocked as it's always complete");
        }

        public void Reset()
        {
        }
    }
}