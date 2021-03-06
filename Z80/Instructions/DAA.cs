//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
namespace Z80.Instructions
{
    public class DecimalAdjustAccumulator : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "DAA";

        public bool IsComplete => true;

        public DecimalAdjustAccumulator(Z80Cpu cpu) 
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
            int accumulatorValue = _cpu.A;
            var flags = _cpu.Flags;
            var correction = 0;

            var negateOperation = flags.HasFlag(Z80Flags.AddSubtract_N);

            if (flags.HasFlag(Z80Flags.HalfCarry_H) || 
                (!negateOperation && (accumulatorValue & 0xf) > 9))
            {
                correction = 0x6;
            }

            if (flags.HasFlag(Z80Flags.Carry_C) ||
                (!negateOperation && accumulatorValue > 0x99)) 
            {
                correction |= 0x60;
                Z80Flags.Carry_C.SetOrReset(_cpu, true);
            }

            accumulatorValue += negateOperation ? -correction : correction;
            Register.A.SetValueOnProcessor(_cpu, (byte)(accumulatorValue & 0xff));

            Z80Flags.Zero_Z.SetOrReset(_cpu, accumulatorValue == 0);
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
        }
    }
}