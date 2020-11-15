using NUnit.Framework;

namespace Z80.Tests.InputOutput
{
    public class OutputAndIncrementTests : CpuRunTestBase {

        [Test]
        public void OutputAndIncrementHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa3;

            _cpu.B = 0x05;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            _ram[0x210a] = 0x5a;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x210b));
            Assert.That(_cpu.B, Is.EqualTo(0x04));
            Assert.That(DataAtIoAddress(0x041d), Is.EqualTo(0x5a));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }

        [Test]
        public void OuputAndIncrementRepeatingHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xb3;

            _cpu.B = 0x03;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            _ram[0x210a] = 0xf7;
            _ram[0x210b] = 0x94;
            _ram[0x210c] = 0x1b;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x210d));
            Assert.That(_cpu.B, Is.EqualTo(0x0));
            Assert.That(DataAtIoAddress(0x021d), Is.EqualTo(0xf7));
            Assert.That(DataAtIoAddress(0x011d), Is.EqualTo(0x94));
            Assert.That(DataAtIoAddress(0x001d), Is.EqualTo(0x1b));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }
    } 
}