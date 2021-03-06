//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions {
    public class HALT : IInstruction
    {
        private readonly Z80Cpu _cpu;

        public string Mnemonic => "HALT";

        public bool IsComplete => true;

        public HALT(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Clock()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
        }

        public void StartExecution()
        {
            _cpu.HALT = true;
        }
    }
}