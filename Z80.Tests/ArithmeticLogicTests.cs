using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public class AddTests : CpuRunTestBase
    {
        [Test]
        public void AddRegisterTest()
        {
            // Arrange
            _ram[0] = 0x81;

            _cpu.A = 12;
            _cpu.C = 40;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(52));
        }

        [Test]
        public void AddImmediateTest()
        {
            // Arrange
            _ram[0] = 0xc6;
            _ram[1] = 70;

            _cpu.A = 4;
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(74));
        }

        [Test]
        public void AddHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x15b0;

            _ram[0] = 0x86;
            _ram[pointerAddress] = 12;

            _cpu.A = 9;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(21));
        }

        [Test]
        public void AddIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0x9c22;

            _ram[0] = 0xdd;
            _ram[1] = 0x86;
            _ram[2] = 3;
            _ram[pointerAddress+3] = 58;

            _cpu.A = 12;
            WideRegister.IX.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(70));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndNegativeFlags() {
            // Arrange
            _ram[0] = 0x84;

            _cpu.A = 127;
            _cpu.H = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }
        
        [Test]
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag() {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 255;
            _cpu.D = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }


        [Test]
        public void AddWithHalfCarrySetsHalfCarryFlag() {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 0xf;
            _cpu.D = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

    public class AddWithCarryTests : CpuRunTestBase
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
            _ram[pointerAddress+5] = 110;

            _cpu.A = 56;
            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(167));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndNegativeFlags() {
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
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag() {
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
        public void AddWithHalfCarrySetsHalfCarryFlag() {
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
}