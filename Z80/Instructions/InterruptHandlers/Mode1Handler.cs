//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions.InterruptHandlers {
    internal class Mode1Handler : RST
    {
        public Mode1Handler(Z80Cpu cpu) : base(cpu, 0x38) {
        }
    }
}