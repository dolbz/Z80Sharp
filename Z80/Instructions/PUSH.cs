//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class PUSH : LD_Generic<ushort>
    {
        public override string Mnemonic => "PUSH";

        public PUSH(Z80Cpu cpu, WideRegister register, int additionalM1TCycles = 1) : base(cpu, new RegIndirectWide(cpu, WideRegister.SP, true), new RegAddrMode16Bit(cpu, register), additionalM1TCycles: additionalM1TCycles)
        {
        }

        public override void Clock()
        {
            base.Clock();

            if (IsComplete)
            {
                _cpu.SP -= 2;
            }
        }
    }
}