//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
namespace Z80.Instructions
{
    public class NegateAccumulator : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "NEG";

        public bool IsComplete => true;

        public NegateAccumulator(Z80Cpu cpu) 
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
            _cpu.A = (byte)((~_cpu.A) + 1);

            Z80Flags.Sign_S.SetOrReset(_cpu, (_cpu.A & 0x80) == 0x80);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, _cpu.A == 0x80);
            Z80Flags.Carry_C.SetOrReset(_cpu, _cpu.A != 0);
            Z80Flags.Zero_Z.SetOrReset(_cpu, _cpu.A == 0);
            // Z80Flags.HalfCarry_H.SetOrReset(_cpu, true); // TODO lookup behaviour for H. Docs aren't very clear

        }
    }
}