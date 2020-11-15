using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Z80.Tests.InputOutput
{
    public class OutputTests : CpuRunTestBase {

        [Test]
        public void CompatibleOutputInstructionHappyPath() {
            // Arrange
            _ram[0] = 0xd3;
            _ram[1] = 0x9a;

            _cpu.A = 0xf4;
            _cpu.Flags = (Z80Flags)0xff;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(DataAtIoAddress(0xf49a), Is.EqualTo(0xf4));
        }

        [Test]
        public void NewOutputInstructionsHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x51;

            _cpu.B = 0x12;
            _cpu.C = 0x61;
            _cpu.D = 0x39;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(DataAtIoAddress(0x1261), Is.EqualTo(0x39));
        }
    } 
}