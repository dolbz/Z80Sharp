//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class AddWithCarryTests_8bit : CpuRunTestBase
    {
        [Test]
        public void AddRegisterWithCarryInTest()
        {
            // Arrange
            _ram[0] = 0x8b;

            _cpu.A = 5;
            _cpu.E = 61;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(67));
        }

        [Test]
        public void AddRegisterNoCarryInTest()
        {
            // Arrange
            _ram[0] = 0x8b;

            _cpu.A = 5;
            _cpu.E = 61;
            Z80Flags.Carry_C.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(66));
        }

        [Test]
        public void AddImmediateTest()
        {
            // Arrange
            _ram[0] = 0xce;
            _ram[1] = 44;

            _cpu.A = 9;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(54));
        }

        [Test]
        public void AddHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x669c;

            _ram[0] = 0x8e;
            _ram[pointerAddress] = 120;

            _cpu.A = 2;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(123));
        }

        [Test]
        public void AddIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0xa332;

            _ram[0] = 0xfd;
            _ram[1] = 0x8e;
            _ram[2] = 5;
            _ram[pointerAddress + 5] = 110;

            _cpu.A = 56;
            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(167));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0x89;

            _cpu.A = 126;
            _cpu.C = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag()
        {
            // Arrange
            _ram[0] = 0x8a;

            _cpu.A = 254;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void AddWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x8a;

            _cpu.A = 0xe;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

    public class AddWithCarryTests_16bit : CpuRunTestBase
    {
        [Test]
        public void AddWithCarryInTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x4a;
 
            WideRegister.BC.SetValueOnProcessor(_cpu, 0x4ac3);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x0);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x4ac4));
        }

        [Test]
        public void AddWithNoCarryInTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x4a;
 
            WideRegister.BC.SetValueOnProcessor(_cpu, 0x4ac3);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x0);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x4ac3));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x5a;
 
            WideRegister.DE.SetValueOnProcessor(_cpu, 0x4ac3);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x4000);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x7a;
 
            WideRegister.SP.SetValueOnProcessor(_cpu, 0xfff0);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x000f);

            _cpu.Flags = Z80Flags.Carry_C;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void AddWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x5a;

            WideRegister.DE.SetValueOnProcessor(_cpu, 0x3400);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x0bff);
            _cpu.Flags = Z80Flags.Carry_C;
            
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }
}