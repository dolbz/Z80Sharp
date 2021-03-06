//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests {
    public class NegateTests : CpuRunTestBase {
        
        [TestCase(0, 0)]
        [TestCase(1, 255)] // 255 is -1 when treating as a signed byte
        [TestCase(127, 129)] // 129 is -127 when treating as a signed byte
        [TestCase(128, 128)] // Can't negate 128 but causes overflow flag to be set
        public void NegateTest(byte initialValue, byte negatedValue) 
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x44;

            Register.A.SetValueOnProcessor(_cpu, initialValue);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(Register.A.GetValue(_cpu), Is.EqualTo(negatedValue));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.EqualTo(_cpu.A == 0));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.EqualTo(_cpu.A >= 0x80));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.EqualTo(_cpu.A != 0));
            // Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.True); //TODO
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.EqualTo(_cpu.A == 0x80));
        }
    }
}