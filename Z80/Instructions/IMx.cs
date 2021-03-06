//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace Z80.Instructions {
    public class IM : IInstruction
    {
        private readonly Z80Cpu _cpu;
        private readonly int _mode;

        public string Mnemonic => $"IM{_mode}";

        public bool IsComplete => true;

        public IM(Z80Cpu cpu, int mode) {
            _cpu = cpu;
            _mode = mode;
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
            _cpu.InterruptMode = _mode;
        }
    }
}