using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class SubtractWithCarryTests : CpuRunTestBase
    {
        [Test]
        public void SubtractWithCarryInRegisterWithCarryInTest()
        {
            // Arrange
            _ram[0] = 0x9b;

            _cpu.A = 120;
            _cpu.E = 70;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(49));
        }

        [Test]
        public void SubtractWithCarryInRegisterNoCarryInTest()
        {
            // Arrange
            _ram[0] = 0x9b;

            _cpu.A = 47;
            _cpu.E = 4;
            Z80Flags.Carry_C.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(43));
        }

        [Test]
        public void SubtractWithCarryInImmediateTest()
        {
            // Arrange
            _ram[0] = 0xde;
            _ram[1] = 2;

            _cpu.A = 9;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(6));
        }

        [Test]
        public void SubtractWithCarryInHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x669c;

            _ram[0] = 0x9e;
            _ram[pointerAddress] = 90;

            _cpu.A = 101;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(10));
        }

        [Test]
        public void SubtractWithCarryInIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0xa332;

            _ram[0] = 0xfd;
            _ram[1] = 0x9e;
            _ram[2] = 9;
            _ram[pointerAddress + 9] = 12;

            _cpu.A = 87;
            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(74));
        }

        [Test]
        public void SubtractWithCarryInWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0x99;

            _cpu.A = 0x81;
            _cpu.C = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            Z80Flags.Sign_S.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractWithCarryInWithZeroResultSetsZeroFlagAndCarryFlag()
        {
            // Arrange
            _ram[0] = 0x9a;

            _cpu.A = 2;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void SubtractWithCarryInWithLessThanZeroResultSetsCarryFlag()
        {
            // Arrange
            _ram[0] = 0x9a;

            _cpu.A = 1;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void SubtractWithCarryInHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x9a;

            _cpu.A = 0xf1;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

    public class SubtractWithCarryTests_16bit : CpuRunTestBase
    {
        [Test]
        public void SubtractWithCarryInRegisterWithCarryInTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x72;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x21c9);
            WideRegister.SP.SetValueOnProcessor(_cpu, 0x1128);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x10a0));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N));
        }

        [Test]
        public void SubtractWithCarryInRegisterNoCarryInTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x72;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x21c9);
            WideRegister.SP.SetValueOnProcessor(_cpu, 0x1128);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x10a1));
        }

        [Test]
        public void SubtractWithCarryInWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x52;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0xffff);
            WideRegister.DE.SetValueOnProcessor(_cpu, 0x7fff);
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            Z80Flags.Sign_S.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractWithCarryInWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x72;
 
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x442d);
            WideRegister.SP.SetValueOnProcessor(_cpu, 0x442c);
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void SubtractWithCarryInWithLessThanZeroResultSetsCarryFlag()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x62;
 
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x442d);
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void SubtractWithCarryInHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0x42;
 
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x4100);
            WideRegister.BC.SetValueOnProcessor(_cpu, 0x2100);
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }
}