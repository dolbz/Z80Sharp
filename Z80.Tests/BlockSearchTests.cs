using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public class BlockSearchTests : CpuRunTestBase
    {

        [Test]
        public void CompareAndIncrementTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa1;

            byte valToFind = 0x92;
            ushort searchAddress = 0x4d94;
            ushort byteCounter = 20;

            _ram[searchAddress] = valToFind;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchAddress + 1));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(byteCounter - 1));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [TestCase(1, false)]
        [TestCase(10, true)]
        public void CompareAndIncrementPvFlagTest(byte byteCounter, bool flagIsSet)
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa1;

            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.EqualTo(flagIsSet));
        }

        [TestCase(2, 2, false)]
        [TestCase(1, 2, true)]
        public void CompareAndIncrementSignFlagTest(byte searchValue, byte actualValue, bool flagIsSet)
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa1;

            ushort searchAddress = 0x19da;
            ushort byteCounter = 7;

            _ram[searchAddress] = actualValue;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, searchValue);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S), Is.EqualTo(flagIsSet));
        }

        [TestCase(0x10, 0x1, true)]
        [TestCase(0xff, 0x1, false)]
        public void CompareAndIncrementHalfCarryFlagTest(byte searchValue, byte actualValue, bool flagIsSet)
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa1;

            ushort searchAddress = 0x19da;
            ushort byteCounter = 7;

            _ram[searchAddress] = actualValue;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, searchValue);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.EqualTo(flagIsSet));
        }

        [Test]
        public void CompareAndIncrementMiscFlagsTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa1;

            var rnd = new Random();
            var randomState = rnd.Next(2) == 0;

            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false); // Ensure N is reset
            Z80Flags.Carry_C.SetOrReset(_cpu, randomState);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.EqualTo(randomState));
        }

        [Test]
        public void CompareAndIncrementRepeatTest_FindMatch()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xb1;

            byte valToFind = 0xa5;
            ushort searchStartAddress = 0x6a31;
            ushort byteCounter = 30;

            _ram[searchStartAddress + 5] = valToFind;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchStartAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchStartAddress + 6));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(byteCounter - 6));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void CompareAndIncrementRepeatTest_NoMatch()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xb1;

            byte valToFind = 0xa5;
            ushort searchStartAddress = 0x6a31;
            ushort byteCounter = 30;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchStartAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchStartAddress + 30));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(0));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.Not.True);
        }

        [Test]
        public void CompareAndDecrementTest()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xa9;

            byte valToFind = 0x4f;
            ushort searchAddress = 0xc332;
            ushort byteCounter = 90;

            _ram[searchAddress] = valToFind;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchAddress - 1));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(byteCounter - 1));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void CompareAndDecrementRepeatTest_FindMatch()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xb9;

            byte valToFind = 0x07;
            ushort searchStartAddress = 0x100a;
            ushort byteCounter = 32;

            _ram[searchStartAddress - 7] = valToFind;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchStartAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchStartAddress - 8));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(byteCounter - 8));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void CompareAndDecrementRepeatTest_NoMatch()
        {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xb9;

            byte valToFind = 0x4a;
            ushort searchStartAddress = 0x2a02;
            ushort byteCounter = 16;

            WideRegister.HL.SetValueOnProcessor(_cpu, searchStartAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, byteCounter);
            Register.A.SetValueOnProcessor(_cpu, valToFind);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(searchStartAddress - 16));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(0));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.Not.True);
        }
    }
}