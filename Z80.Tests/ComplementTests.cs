using NUnit.Framework;

namespace Z80.Tests {
    public class ComplementTests : CpuRunTestBase {
        [Test]
        public void TestComplement() 
        {
            // Arrange
            _ram[0] = 0x2f;

            Register.A.SetValueOnProcessor(_cpu, 0xaa);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(Register.A.GetValue(_cpu), Is.EqualTo(0x55));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.True);
        }
    }
}