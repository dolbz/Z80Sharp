using System;
using NUnit.Framework;

namespace Z80.Tests
{
    public class AddTests : CpuRunTestBase
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
        public void AddWithOverflowSetsOverflowAndNegativeFlags()
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
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag()
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
    }

    public class AddWithCarryTests : CpuRunTestBase
    {
        [Test]
        public void AddRegisterWithCarryInTest()
        {
            // Arrange
            _ram[0] = 0x8b;

            _cpu.A = 5;
            _cpu.E = 61;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(67));
        }

        [Test]
        public void AddRegisterNoCarryInTest()
        {
            // Arrange
            _ram[0] = 0x8b;

            _cpu.A = 5;
            _cpu.E = 61;
            Z80Flags.Carry_C.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(66));
        }

        [Test]
        public void AddImmediateTest()
        {
            // Arrange
            _ram[0] = 0xce;
            _ram[1] = 44;

            _cpu.A = 9;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(54));
        }

        [Test]
        public void AddHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0x669c;

            _ram[0] = 0x8e;
            _ram[pointerAddress] = 120;

            _cpu.A = 2;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(123));
        }

        [Test]
        public void AddIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0xa332;

            _ram[0] = 0xfd;
            _ram[1] = 0x8e;
            _ram[2] = 5;
            _ram[pointerAddress + 5] = 110;

            _cpu.A = 56;
            WideRegister.IY.SetValueOnProcessor(_cpu, pointerAddress);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(167));
        }

        [Test]
        public void AddWithOverflowSetsOverflowAndNegativeFlags()
        {
            // Arrange
            _ram[0] = 0x89;

            _cpu.A = 126;
            _cpu.C = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void AddWithZeroResultSetsZeroFlagAndCarryFlag()
        {
            // Arrange
            _ram[0] = 0x8a;

            _cpu.A = 254;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void AddWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x8a;

            _cpu.A = 0xe;
            _cpu.D = 1;
            _cpu.Flags = 0;
            Z80Flags.Carry_C.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

    public class SubtractTests : CpuRunTestBase
    {
        [Test]
        public void SubtractRegisterTest()
        {
            // Arrange
            _ram[0] = 0x90;

            _cpu.A = 32;
            _cpu.B = 9;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(23));
        }

        [Test]
        public void SubtractImmediateTest()
        {
            // Arrange
            _ram[0] = 0xd6;
            _ram[1] = 70;

            _cpu.A = 97;
            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(27));
        }

        [Test]
        public void SubtractHLPointerTest()
        {
            // Arrange
            ushort pointerAddress = 0xb0a7;

            _ram[0] = 0x96;
            _ram[pointerAddress] = 41;

            _cpu.A = 92;
            WideRegister.HL.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(51));
        }

        [Test]
        public void SubtractIndexedTest()
        {
            // Arrange
            ushort pointerAddress = 0xf19a;

            _ram[0] = 0xdd;
            _ram[1] = 0x96;
            _ram[2] = 5;
            _ram[pointerAddress + 5] = 22;

            _cpu.A = 127;
            WideRegister.IX.SetValueOnProcessor(_cpu, pointerAddress);
            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(105));
        }

        [Test]
        public void SubtractWithOverflowSetsOverflowAndNegativeFlags()
        {
            // Arrange
            _ram[0] = 0x94;

            _cpu.A = 0x0;
            _cpu.H = 1;
            _cpu.Flags = 0;
            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractWithZeroResultSetsZeroFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 1;
            _cpu.D = 1;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void SubtractWithLessThanZeroResultSetsCarryFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 1;
            _cpu.D = 2;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }

        [Test]
        public void SubtractWithHalfCarrySetsHalfCarryFlag()
        {
            // Arrange
            _ram[0] = 0x92;

            _cpu.A = 0xf0;
            _cpu.D = 1;
            _cpu.Flags = 0;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }
    }

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
        public void SubtractWithCarryInWithOverflowSetsOverflowAndNegativeFlags()
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

    public class ANDTests : CpuRunTestBase
    {
        [Test]
        public void ANDTest()
        {
            // Arrange
            _ram[0] = 0xa3;

            _cpu.A = 0x5a;
            _cpu.E = 0x24;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0x5a & 0x24));
        }

        [Test]
        public void ANDFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x0;
            _cpu.H = 0x0;

            // Set fixed flags to the opposite of what they should be after execution
            _cpu.Flags = Z80Flags.AddSubtract_N | Z80Flags.Carry_C;
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.True);
        }

        [Test]
        public void ANDWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x8f;
            _cpu.H = 0x03;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void ANDWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0x8f;
            _cpu.H = 0x02;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void ANDResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0xff;
            _cpu.H = 0x00;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void ANDResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xa4;

            _cpu.A = 0xff;
            _cpu.H = 0xf1;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }

    public class ORTests : CpuRunTestBase
    {
        [Test]
        public void ORTest()
        {
            // Arrange
            _ram[0] = 0xb1;

            _cpu.A = 0b11001100;
            _cpu.C = 0b10000110;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b11001110));
        }

        [Test]
        public void ORFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x0;
            _cpu.H = 0x0;

            // Set fixed flags to the opposite of what they should be after execution
            _cpu.Flags = Z80Flags.AddSubtract_N | Z80Flags.Carry_C | Z80Flags.HalfCarry_H;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.True);
        }

        [Test]
        public void ORWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0b01101000;
            _cpu.H = 0b11101000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void ORWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0b01100001;
            _cpu.H = 0b10010000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void ORResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x00;
            _cpu.H = 0x00;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void ORResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xb4;

            _cpu.A = 0x00;
            _cpu.H = 0x80;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }

    public class XORTests : CpuRunTestBase
    {
        [Test]
        public void XORTest()
        {
            // Arrange
            _ram[0] = 0xaa;

            _cpu.A = 0b11001100;
            _cpu.D = 0b10000110;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(0b01001010));
        }

        [Test]
        public void XORFixedFlagsTest()
        {
            // Arrange
            _ram[0] = 0xa9;

            _cpu.A = 0x0;
            _cpu.C = 0x0;

            // Set fixed flags to the opposite of what they should be after execution
            _cpu.Flags = Z80Flags.AddSubtract_N | Z80Flags.Carry_C | Z80Flags.HalfCarry_H;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.Not.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.Not.True);
        }

        [Test]
        public void XORWithEvenParityResult_ParityFlagIsSet()
        {
            // Arrange
            _ram[0] = 0xad;

            _cpu.A = 0b01101000;
            _cpu.L = 0b11100000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void XORWithOddParityResult_ParityFlagIsNotSet()
        {
            // Arrange
            _ram[0] = 0xa8;

            _cpu.A = 0b01100001;
            _cpu.B = 0b11110000;

            Z80Flags.ParityOverflow_PV.SetOrReset(_cpu, true);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV), Is.Not.True);
        }

        [Test]
        public void XORResultsInZeroFlagSet()
        {
            // Arrange
            _ram[0] = 0xac;

            _cpu.A = 0b11001010;
            _cpu.H = 0b11001010;

            Z80Flags.Zero_Z.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void XORResultsInSignFlagSet()
        {
            // Arrange
            _ram[0] = 0xac;

            _cpu.A = 0b00001011;
            _cpu.H = 0b10001001;

            Z80Flags.Sign_S.SetOrReset(_cpu, false);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }
    }

    public class CompareTests : CpuRunTestBase
    { 
        [Test]
        public void ComparedValuesAreEqual() {
            // Arrange
            _ram[0] = 0xb8;

            _cpu.A = 0x43;
            _cpu.B = 0x43;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z));
        }

        [Test]
        public void ComparedValuesAreNotEqual() {
            // Arrange
            _ram[0] = 0xb8;

            _cpu.A = 0x9a;
            _cpu.B = 0x43;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Iz.Not.True);
        }

        [Test]
        public void SignFlagIsSetWhenResultIsNegative() {
            // Arrange
            _ram[0] = 0xba;

            _cpu.A = 0x9a;
            _cpu.D = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Sign_S));
        }

        [Test]
        public void HalfCarryFlagIsSetOnHalfCarry() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x10;
            _cpu.E = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.HalfCarry_H));
        }

        [Test]
        public void OverflowFlagIsSetOnOverflow() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x4;
            _cpu.E = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.ParityOverflow_PV));
        }

        [Test]
        public void SubtractFlagIsSet() 
        {
            // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x4;
            _cpu.E = 0xa5;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N));
        }

        [Test]
        public void CarryFlagIsSetOnCarry() 
        {
                        // Arrange
            _ram[0] = 0xbb;

            _cpu.A = 0x0b;
            _cpu.E = 0x15;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }
    }
}