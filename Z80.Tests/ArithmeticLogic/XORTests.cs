//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class XORTests : CpuRunTestBase
    {
        [Test]
        public void XORTest()
        {
            // Arrange
            _ram[0] = 0xaa;

            _cpu.A = 0b11001100;
            _cpu.D = 0b10000110;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b01001010));
        }

        [Test]
        public void XORFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xa9;

            _cpu.A = 0x0;
            _cpu.C = 0x0;

            // Set fixed flags to the opposite of what they should be after execution
            _cpu.Flags = Z80Flags.AddSubtract_N | Z80Flags.Carry_C | Z80Flags.HalfCarry_H;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.True);
        }

        [Test]
        public void XORWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xad;

            _cpu.A = 0b01101000;
            _cpu.L = 0b11100000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void XORWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xa8;

            _cpu.A = 0b01100001;
            _cpu.B = 0b11110000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void XORResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xac;

            _cpu.A = 0b11001010;
            _cpu.H = 0b11001010;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void XORResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xac;

            _cpu.A = 0b00001011;
            _cpu.H = 0b10001001;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }
}