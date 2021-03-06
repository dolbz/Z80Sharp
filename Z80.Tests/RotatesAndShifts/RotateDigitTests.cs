//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.RotatesAndShifts 
{
    public class RotateDigitTests : CpuRunTestBase 
    {
        [Test]
        public void RotateLeftDigit() 
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x6f;
            _ram[0xa51a] = 0xf7;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0xa51a);
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Carry_C | Z80Flags.Sign_S | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N | Z80Flags.ParityOverflow_PV;
            _cpu.A = 0x43;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[0xa51a], Is.EqualTo(0x73));
            Assert.That(_cpu.A, Is.EqualTo(0x4f));

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }

        [Test]
        public void RotateRightDigit() 
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x67;
            _ram[0x951a] = 0x3a;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x951a);
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Carry_C | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N;
            _cpu.A = 0x9d;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[0x951a], Is.EqualTo(0xd3));
            Assert.That(_cpu.A, Is.EqualTo(0x9a));

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.True); // Unchanged

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }
    }
}