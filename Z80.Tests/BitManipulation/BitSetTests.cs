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

        [Test]
        public void BitSetIndexed() {
            // Arrange
            _ram[0] = 0xdd;
            _ram[1] = 0xcb; // SET 3,(IX+d)
            _ram[2] = 6;
            _ram[3] = 0xde;
            _ram[0x41c7] = 0b11010001;

            WideRegister.IX.SetValueOnProcessor(_cpu, 0x41c1);

            // Act
            RunUntil(6);

            // Assert
            Assert.That(_ram[0x41c7], Is.EqualTo(0b11011001));
        }
    }
}