using NUnit.Framework;

namespace Z80.Tests.BitManipulation 
{
    public class BitSetTests : CpuRunTestBase {
        [Test]
        public void BitSetRegister() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0xd9; // SET 3,C
            _cpu.C = 0b11010001;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.C, Is.EqualTo(0b11011001));
        }

        [Test]
        public void BitSetHLIndirect() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0xee; // SET 5,(HL)
            _ram[0x2d22] = 0b11011111;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x2d22);
            _cpu.Flags = Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[0x2d22], Is.EqualTo(0b11111111));
        }
    }
}