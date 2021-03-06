//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class ANDTests : CpuRunTestBase
    {
        [Test]
        public void ANDTest()
        {
            // Arrange
            _ram[0] = 0xa3;

            _cpu.A = 0x5a;
            _cpu.E = 0x24;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0x5a & 0x24));
        }

        [Test]
        public void ANDFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x0;
            _cpu.H = 0x0;

            // Set fixed flags to the opposite of what they should be after execution
            _cpu.Flags = Z80Flags.AddSubtract_N | Z80Flags.Carry_C;
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.True);
        }

        [Test]
        public void ANDWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x8f;
            _cpu.H = 0x03;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void ANDWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x8f;
            _cpu.H = 0x02;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void ANDResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0xff;
            _cpu.H = 0x00;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void ANDResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0xff;
            _cpu.H = 0xf1;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }
}