using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class DecrementsTests_8bit : CpuRunTestBase
    {
        [Test]
        public void DecrementRegisterTest()
        {
            // Arrange
            _ram[0] = 0x0d;

            _cpu.C = 0x12;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.C, Is.EqualTo(0x11));
        }

        [Test]
        public void DecrementHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x19f3;

            _ram[0] = 0x35;
            _ram[pointerAddress] = 77;

            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_ram[pointerAddress], Is.EqualTo(76));
        }

        [Test]
        public void DecrementIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0x224d;

            _ram[0] = 0xfd;
            _ram[1] = 0x35;
            _ram[2] = 3;
            _ram[pointerAddress + 3] = 33;

            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_ram[pointerAddress+3], Is.EqualTo(32));
        }

        [Test]
        public void DecrementWithOverflowSetsOverflowAndResetsSignFlags()
        {
            // Arrange
            _ram[0] = 0x1d;

            _cpu.E = 0x80;
            _cpu.Flags = Z80Flags.Sign_S;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void DecrementWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0x1d;

            _cpu.E = 1;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void DecrementWithCarryOutDoesNotChangeCarryFlag()
        {
            // Arrange
            _ram[0] = 0x1d;

            _cpu.E = 0;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);
            Assert.That(_cpu.E, Is.EqualTo(0xff));
        }


        [Test]
        public void DecrementWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x25;

            _cpu.H = 0x40;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

    public class DecrementsTests_16bit : CpuRunTestBase
    {
        [Test]
        public void DecrementRegisterTest()
        {
            // Arrange
            _ram[0] = 0x3b;

            WideRegister.SP.SetValueOnProcessor(_cpu, 0x8214);
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.SP.GetValue(_cpu), Is.EqualTo(0x8213));
        }

        [Test]
        public void DecrementDoesntAffectFlags()
        {
            // Arrange
            _ram[0] = 0x3b;

            WideRegister.SP.SetValueOnProcessor(_cpu, 0x8000);
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.SP.GetValue(_cpu), Is.EqualTo(0x7fff));
            Assert.That(_cpu.Flags, Is.EqualTo((Z80Flags)0x0));
        }
    }
}