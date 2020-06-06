using NUnit.Framework;

namespace Z80.Tests.ArithmeticLogic
{
    public class AddTests_8Bit : CpuRunTestBase
    {
        [Test]
        public void AddRegisterTest()
        {
            // Arrange
            _ram[0] = 0x81;

            _cpu.A = 12;
            _cpu.C = 40;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(52));
        }

        [Test]
        public void AddImmediateTest()
        {
            // Arrange
            _ram[0] = 0xc6;
            _ram[1] = 70;

            _cpu.A = 4;
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(74));
        }

        [Test]
        public void AddHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x15b0;

            _ram[0] = 0x86;
            _ram[pointerAddress] = 12;

            _cpu.A = 9;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(21));
        }

        [Test]
        public void AddIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0x9c22;

            _ram[0] = 0xdd;
            _ram[1] = 0x86;
            _ram[2] = 3;
            _ram[pointerAddress + 3] = 58;

            _cpu.A = 12;
            WideRegister.IX.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(70));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndSignFlags()
        {
            // Arrange
            _ram[0] = 0x84;

            _cpu.A = 127;
            _cpu.H = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void AddWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 255;
            _cpu.D = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void AddWithCarryOutSetsCarryFlag()
        {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 0x80;
            _cpu.D = 0xff;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }


        [Test]
        public void AddWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 0xf;
            _cpu.D = 1;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }

        [Test]
        public void AddWithoutHalfCarryDoesntSetHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x82;

            _cpu.A = 0x9;
            _cpu.D = 1;
            _cpu.Flags = 0;
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.Not.True);
        }
    }

    public class AddTests_16Bit : CpuRunTestBase
    {
        [Test]
        public void AddToHL()
        {
            // Arrange
            _ram[0] = 0x09;

            WideRegister.BC.SetValueOnProcessor(_cpu, 0x23fc);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x5e94);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x8290));
        }

       [Test]
        public void AddWithUpperByteHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x19;

            WideRegister.DE.SetValueOnProcessor(_cpu, 0x3400);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x0c00);
            _cpu.Flags = 0;
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }

        [Test]
        public void AddWithCarryOut()
        {
            // Arrange
            _ram[0] = 0x19;

            WideRegister.DE.SetValueOnProcessor(_cpu, 0x3000);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0xd000);
            _cpu.Flags = 0;
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void AddResetsNFlag()
        {
            // Arrange
            _ram[0] = 0x19;

            WideRegister.DE.SetValueOnProcessor(_cpu, 0x3000);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0xd000);
            _cpu.Flags = Z80Flags.AddSubtract_N;
            
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
        }
    }
}