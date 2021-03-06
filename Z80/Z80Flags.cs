//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;

namespace Z80 {
    [Flags]
    public enum Z80Flags : byte
    {
        Sign_S = 128,
        Zero_Z = 64,
        HalfCarry_H = 16,
        ParityOverflow_PV = 4,
        AddSubtract_N = 2,
        Carry_C = 1
    }

    public static class Z80FlagsExtensions {
        public static void SetOrReset(this Z80Flags flag, Z80Cpu cpu, bool value) {
            if (value) {
                cpu.Flags |= flag;
            } else {
                cpu.Flags &= ~flag;
            }
        }
    }
}