using NUnit.Framework;

namespace Z80.Tests.RotatesAndShifts 
{
    public class Interl8080CompatibleRotateTests : CpuRunTestBase 
    {
        [Test]
        public void RotateLeftCircularTest() {
            // Arrange
            _ram[0] = 0x07;

            _cpu.A = 0b10011000;
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Sign_S | Z80Flags.ParityOverflow_PV | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b00110001));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.True);

            CheckCommonFlags();
        }

        [Test]
        public void RotateRightCircularTest() {
            // Arrange
            _ram[0] = 0x0f;

            _cpu.A = 0b10011000;
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Sign_S | Z80Flags.ParityOverflow_PV | Z80Flags.HalfCarry_H | Z80Flags.Carry_C | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b01001100));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);

            CheckCommonFlags();
        }

        [Test]
        public void RotateLeftTest() {
            // Arrange
            _ram[0] = 0x17;

            _cpu.A = 0b10011000;
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Sign_S | Z80Flags.ParityOverflow_PV | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b00110000));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.True);

            CheckCommonFlags();
        }

        [Test]
        public void RotateRightTest() {
            // Arrange
            _ram[0] = 0x1f;

            _cpu.A = 0b10011000;
            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.Sign_S | Z80Flags.ParityOverflow_PV | Z80Flags.HalfCarry_H | Z80Flags.Carry_C | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b11001100));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);

            CheckCommonFlags();
        }

        private void CheckCommonFlags() {
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.True); // Flag is unaffected
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True); // Flag is unaffected
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.True); // Flag is unaffected

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }
    }
}