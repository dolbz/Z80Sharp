//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests {
    public class ComplementCarryFlagTests : CpuRunTestBase {
        
        [TestCase(true)]
        [TestCase(false)]
        public void CarryFlagIsComplemented(bool carryAlreadySet) 
        {
            // Arrange
            _ram[0] = 0x3f;

            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            if (carryAlreadySet) {
                Z80Flags.Carry_C.SetOrReset(_cpu, true);
            }

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.EqualTo(carryAlreadySet));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.EqualTo(carryAlreadySet));
        }
    }
}