using NUnit.Framework;

namespace Z80.Tests {
    public class CpuControlTests : CpuRunTestBase {
        
        [Test]
        public void EnableInterrupt() {
            // Arrange
            _ram[0] = 0xfb; // EI

            _cpu.IFF1 = false;
            _cpu.IFF2 = false;

            // Act
            RunUntil(1);
            
            // Assert
            Assert.That(_cpu.IFF1, Is.False);
            Assert.That(_cpu.IFF2, Is.False);

            // Act 2
            RunUntil(2);

            // Assert 2 - EI behaviour occurs after the instruction following EI
            Assert.That(_cpu.IFF1, Is.True);
            Assert.That(_cpu.IFF2, Is.True);

        }

        [Test]
        public void DisableInterrupt() {
            // Arrange
            _ram[0] = 0xf3; // DI

            _cpu.IFF1 = true;
            _cpu.IFF2 = true;

            // Act
            RunUntil(1);
            
            // Assert
            Assert.That(_cpu.IFF1, Is.False);
            Assert.That(_cpu.IFF2, Is.False);
        }

        [Test]
        public void Halt() {
            // Arrange
            _ram[0] = 0x76; // HALT

            _cpu.HALT  = false;

            // Act
            RunUntil(1);
            for(int i = 0; i < 100; i++) {
                _cpu.Clock(); // Additional clock cycles would normal advance the PC if we weren't HALTed
            }
            
            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(1));
            Assert.That(_cpu.HALT, Is.True);
        }

        [TestCase(0x46, 0)]
        [TestCase(0x56, 1)]
        [TestCase(0x5e, 2)]
        public void SetInterrupMode(int opcode, int mode) {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = (byte)opcode;

            _cpu.InterruptMode = -1;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.InterruptMode, Is.EqualTo(mode));
        }
    }
}