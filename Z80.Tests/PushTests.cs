using NUnit.Framework;

namespace Z80.Tests
{
    public class PushTests : CpuRunTestBase {
        [Test]
        public void PushAF() {
            // Arrange
            _cpu.SP = 0xbeef;
        
            byte msn = 0x31;
            byte lsn = 0x9c;

            var expectedStackPointer = _cpu.SP - 2;

            _ram[0] = 0xf5;
            _cpu.A = msn;
            _cpu.Flags = (Z80Flags)lsn;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[expectedStackPointer], Is.EqualTo(lsn));
            Assert.That(_ram[expectedStackPointer+1], Is.EqualTo(msn));
        }
    }
}