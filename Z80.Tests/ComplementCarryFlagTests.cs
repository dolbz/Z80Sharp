using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests {
    public class ComplementCarryFlagTests : CpuRunTestBase {
        
        [TestCase(true)]
        [TestCase(false)]
        public void CarryFlagIsComplemented(bool carryAlreadySet) 
        {
            // Arrange
            _ram[0] = 0x3f;

            if (carryAlreadySet) {
                _cpu.Flags = Z80Flags.Carry_C;
            }

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.EqualTo(carryAlreadySet));
        }
    }
}