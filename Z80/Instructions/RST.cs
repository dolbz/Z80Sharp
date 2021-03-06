//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using Z80.AddressingModes;

namespace Z80.Instructions {
    internal class RST : CALL {
        public override string Mnemonic => "RST";
        public RST(Z80Cpu cpu, ushort jumpValue) : base(cpu, new StaticAddressMode(jumpValue), JumpCondition.Unconditional) {

        }
    }
}