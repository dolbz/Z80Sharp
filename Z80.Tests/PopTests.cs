using NUnit.Framework;

namespace Z80.Tests
{
    public class PopTests : CpuRunTestBase {
        [Test]
        public void PopBC() {
            // Arrange
            _cpu.SP = 0xb00b;
        
            byte msn = 0x74;
            byte lsn = 0x1c;

            var expectedStackPointer = _cpu.SP - 2;

            _ram[0] = 0xc1;
            _ram[_cpu.SP] = lsn;
            _ram[_cpu.SP+1] = msn;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo((msn << 8) | lsn));
        }
    }
}