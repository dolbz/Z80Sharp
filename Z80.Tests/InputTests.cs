using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Z80.Tests
{
    public class InputTests : CpuRunTestBase {

        [Test]
        public void CompatibleInputInstructionSetsRegisterAndDoesntSetFlags() {
            // Arrange
            _ram[0] = 0xdb;
            _ram[1] = 0x9a;

            _cpu.A = 0xf4;
            _cpu.Flags = (Z80Flags)0xff;

            AddDataAtIoAddress(0xf49a, 0x69);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0x69));
            Assert.That(_cpu.Flags, Is.EqualTo((Z80Flags)0xff));
        }

        [Test]
        public void NewInputInstructionsSetsChosenRegisterAndSetsFlags() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x50;

            _cpu.B = 0x12;
            _cpu.C = 0x61;
            _cpu.Flags = (Z80Flags)0xff;

            AddDataAtIoAddress(0x1261, 0x22);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.D, Is.EqualTo(0x22));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }
    } 
}