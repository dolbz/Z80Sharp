using NUnit.Framework;

namespace Z80.Tests.RotatesAndShifts 
{
    public class ShiftRightArithmeticTests : CpuRunTestBase 
    {
        [Test]
        public void ShiftRegister() 
        {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x2b;

            _cpu.E = 0b10011000;
            _cpu.Flags = Z80Flags.Carry_C | Z80Flags.Zero_Z | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.E, Is.EqualTo(0b11001100));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }

        [Test]
        public void ShiftHLPointedData() 
        {
            // Arrange
            _ram[0] = 0xcb;
            _ram[1] = 0x2e;
            _ram[0x340a] = 0b01010011;

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x340a);
            _cpu.Flags = Z80Flags.ParityOverflow_PV | Z80Flags.Sign_S | Z80Flags.Zero_Z | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[0x340a], Is.EqualTo(0b00101001));

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.True);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }

        [Test]
        public void ShiftIndexedPointedData() 
        {
            // Arrange
            _ram[0] = 0xdd;
            _ram[1] = 0xcb;
            _ram[2] = 0x03;
            _ram[3] = 0x2e; // SRA
            _ram[0xa403] = 0b10100110;

            WideRegister.IX.SetValueOnProcessor(_cpu, 0xa400);
            _cpu.Flags = Z80Flags.Carry_C | Z80Flags.Zero_Z | Z80Flags.HalfCarry_H | Z80Flags.AddSubtract_N | Z80Flags.ParityOverflow_PV;

            // Act
            RunUntil(6);

            // Assert
            Assert.That(_ram[0xa403], Is.EqualTo(0b11010011));

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.False);

            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.False); // Flag is reset
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.False); // Flag is reset
        }
    }
}