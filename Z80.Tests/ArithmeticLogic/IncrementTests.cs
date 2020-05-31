using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class IncrementsTests : CpuRunTestBase
    {
        [Test]
        public void IncrementRegisterTest()
        {
            // Arrange
            _ram[0] = 0x0c;

            _cpu.C = 0xf2;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.C, Is.EqualTo(0xf3));
        }

        [Test]
        public void IncrementHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0xddc2;

            _ram[0] = 0x34;
            _ram[pointerAddress] = 90;

            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_ram[pointerAddress], Is.EqualTo(91));
        }

        [Test]
        public void IncrementIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0x9c22;

            _ram[0] = 0xfd;
            _ram[1] = 0x34;
            _ram[2] = 2;
            _ram[pointerAddress + 2] = 13;

            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_ram[pointerAddress+2], Is.EqualTo(14));
        }

        [Test]
        public void IncrementWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0x1c;

            _cpu.E = 127;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void IncrementWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0x1c;

            _cpu.E = 255;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void IncrementWithCarryOutDoesNotChangeCarryFlag()
        {
            // Arrange
            _ram[0] = 0x1c;

            _cpu.E = 255;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);
            Assert.That(_cpu.E, Is.EqualTo(0));
        }


        [Test]
        public void IncrementWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x24;

            _cpu.H = 0xf;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }
}