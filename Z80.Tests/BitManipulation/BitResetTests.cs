using NUnit.Framework;

namespace Z80.Tests.BitManipulation 
{
    public class BitResetTests : CpuRunTestBase {
        [Test]
        public void BitResetRegister() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x95; // RST 2,L
            _cpu.L = 0b11010111;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.L, Is.EqualTo(0b11010011));
        }

        [Test]
        public void BitResetHLIndirect() {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0xb6; // RST 6,(HL)
            _ram[0x70fc] = 0b11011111;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x70fc);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[0x70fc], Is.EqualTo(0b10011111));
        }
    }
}