using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class SubtractTests : CpuRunTestBase
    {
        [Test]
        public void SubtractRegisterTest()
        {
            // Arrange
            _ram[0] = 0x90;

            _cpu.A = 32;
            _cpu.B = 9;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(23));
        }

        [Test]
        public void SubtractImmediateTest()
        {
            // Arrange
            _ram[0] = 0xd6;
            _ram[1] = 70;

            _cpu.A = 97;
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(27));
        }

        [Test]
        public void SubtractHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0xb0a7;

            _ram[0] = 0x96;
            _ram[pointerAddress] = 41;

            _cpu.A = 92;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(51));
        }

        [Test]
        public void SubtractIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0xf19a;

            _ram[0] = 0xdd;
            _ram[1] = 0x96;
            _ram[2] = 5;
            _ram[pointerAddress + 5] = 22;

            _cpu.A = 127;
            WideRegister.IX.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(105));
        }

        [Test]
        public void SubtractWithNegativeResultSetsSignFlag()
        {
            // Arrange
            _ram[0] = 0x94;

            _cpu.A = 0x0;
            _cpu.H = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }

        [Test]
        public void SubtractWithOverflowSetsOverflowFlag()
        {
            // Arrange
            _ram[0] = 0x94;

            _cpu.A = 0x1;
            _cpu.H = 0x80;
            _cpu.Flags = 0;
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 1;
            _cpu.D = 1;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void SubtractWithLessThanZeroResultSetsCarryFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 1;
            _cpu.D = 2;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void SubtractWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 0x8;
            _cpu.D = 9;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }

        [Test]
        public void SubtractWithoutHalfCarryDoesNotSetHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 0x6;
            _cpu.D = 5;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.Not.True);
        }
    }
}