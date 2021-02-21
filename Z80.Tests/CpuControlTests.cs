using NUnit.Framework;

namespace Z80.Tests {
    public class CpuControlTests : CpuRunTestBase {
        
        [Test]
        public void EnableInterrupt() {
            // Arrange
            _ram[0] = 0xfb; // EI

            _cpu.INT = true;
            _cpu.IFF1 = false;
            _cpu.IFF2 = false;
            _cpu.InterruptMode = 1;

            _ram[1] = 0x3e; // LD A,&c4
            _ram[2] = 0xc4;
            _ram[3] = 0x3e; // LD A,&ff
            _ram[4] = 0xff;

            // Act
            RunUntil(0x38);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0xc4)); // Confirms that LD A,&c4 has run and not the subsequent LD instruction
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(24)); // Confirm we've jumped to 0x38 rather than run sequentially to it somehow
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
            _cpu.Data = _ram[0];

            // Act
            for(int i = 0; i < 100; i++) {
                _cpu.Clock(); // Additional clock cycles would normal advance the PC if we weren't HALTed
            }
            
            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0));
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