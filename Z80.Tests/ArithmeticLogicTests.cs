using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public class ArithmeticLogicTests : CpuRunTestBase
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
}