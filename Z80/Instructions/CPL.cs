//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
namespace Z80.Instructions
{
    public class ComplementAccumulator : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "CPL";

        public bool IsComplete => true;

        public ComplementAccumulator(Z80Cpu cpu) 
        {
            _cpu = cpu;
        }

        public void Clock()
        {
            throw new InvalidOperationException();
        }

        public void Reset()
        {
        }

        public void StartExecution()
        {
            _cpu.A = (byte)(~_cpu.A);

            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, true);
        }
    }
}