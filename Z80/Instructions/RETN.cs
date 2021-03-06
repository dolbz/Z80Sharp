//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions {
    internal class RETN : RET {
        public RETN(Z80Cpu cpu) : base(cpu, JumpCondition.Unconditional) {

        }

        public override string Mnemonic => "RETN";
        public override void Clock()
        {
            base.Clock();
            if (IsComplete) {
                _cpu.IFF1 = _cpu.IFF2;
            }
        }
    }
}