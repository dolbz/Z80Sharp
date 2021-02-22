using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class CompareTests : CpuRunTestBase
    { 
        [Test]
        public void ComparedValuesAreEqual() {
            // Arrange
            _ram[0] = 0xb8;

            _cpu.A = 0x43;
            _cpu.B = 0x43;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void AccumulatorValueIsNotChangedOnExecution() {
            // Arrange
            _ram[0] = 0xb8;

            _cpu.A = 0x43;
            _cpu.B = 0xa4;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0x43));
        }

        [Test]
        public void ComparedValuesAreNotEqual() {
            // Arrange
            _ram[0] = 0xb8;

            _cpu.A = 0x9a;
            _cpu.B = 0x43;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Iz.Not.True);
        }

        [Test]
        public void SignFlagIsSetWhenResultIsNegative() {
            // Arrange
            _ram[0] = 0xba;

            _cpu.A = 0x9a;
            _cpu.D = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }

        [Test]
        public void HalfCarryFlagIsSetOnHalfCarry() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x10;
            _cpu.E = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }

        [Test]
        public void OverflowFlagIsSetOnOverflow() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0xfe;
            _cpu.E = 0x7f;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractFlagIsSet() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x4;
            _cpu.E = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N));
        }

        [Test]
        public void CarryFlagIsSetOnCarry() 
        {
                        // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x0b;
            _cpu.E = 0x15;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }
    }
}