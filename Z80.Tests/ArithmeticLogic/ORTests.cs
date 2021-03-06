//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class ORTests : CpuRunTestBase
    {
        [Test]
        public void ORTest()
        {
            // Arrange
            _ram[0] = 0xb1;

            _cpu.A = 0b11001100;
            _cpu.C = 0b10000110;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b11001110));
        }

        [Test]
        public void ORFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x0;
            _cpu.H = 0x0;

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
        public void ORWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0b01101000;
            _cpu.H = 0b11101000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void ORWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0b01100001;
            _cpu.H = 0b10010000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void ORResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x00;
            _cpu.H = 0x00;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void ORResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x00;
            _cpu.H = 0x80;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }
}