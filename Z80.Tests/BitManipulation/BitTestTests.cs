using NUnit.Framework;

namespace Z80.Tests.BitManipulation 
{
    public class BitTestTests : CpuRunTestBase {
        [Test]
        public void BitTestRegister_BitIsSet() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x47; // BIT 0,A
            _cpu.A = 0b11011001;

            _cpu.Flags = Z80Flags.Zero_Z | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.True); // Flag is set
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }

        [Test]
        public void BitTestRegister_BitIsNotSet() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x62; // BIT 4,D
            _cpu.D = 0b11101111;

            _cpu.Flags = Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.True); // Flag is set
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }

        [Test]
        public void BitTestHLIndirect_BitIsNotSet() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x76; // BIT 6,(HL)
            _ram[0x119d] = 0b10111111;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x119d);
            _cpu.Flags = Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.True); // Flag is set
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }
    }
}