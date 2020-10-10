using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests.JumpCallReturn {
    public class JumpTests : CpuRunTestBase {
        
        [Test]
        public void JumpUnconditional() {
            // Arrange
            _ram[0] = 0xc3; // JP 0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfCarry_ShouldJump() {
            // Arrange
            _ram[0] = 0xda; // JP C,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfCarry_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xda; // JP C,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfNonCarry_ShouldJump() {
            // Arrange
            _ram[0] = 0xd2; // JP NC,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfNonCarry_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xd2; // JP NC,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfZero_ShouldJump() {
            // Arrange
            _ram[0] = 0xca; // JP Z,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfZero_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xca; // JP Z,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfNonZero_ShouldJump() {
            // Arrange
            _ram[0] = 0xc2; // JP NZ,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIfNonZero_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xc2; // JP NZ,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpParityEven_ShouldJump() {
            // Arrange
            _ram[0] = 0xea; // JP PE,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpParityEven_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xea; // JP PE,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpParityOdd_ShouldJump() {
            // Arrange
            _ram[0] = 0xe2; // JP PO,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpParityOdd_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xe2; // JP PO,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpSignNegative_ShouldJump() {
            // Arrange
            _ram[0] = 0xfa; // JP M,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpSignNegative_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xfa; // JP M,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpSignPositive_ShouldJump() {
            // Arrange
            _ram[0] = 0xf2; // JP P,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpSignPositive_ShouldNotJump() {
            // Arrange
            _ram[0] = 0xf2; // JP P,0x49ed
            _ram[1] = 0xed;
            _ram[2] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(3));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void JumpIndirect() {
            // Arrange
            _ram[0] = 0xe9; // JP (HL)

            WideRegister.HL.SetValueOnProcessor(_cpu, 0x49ed);

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(4));
        }

        [Test]
        public void JumpIndexedIndirect() {
            // Arrange
            _ram[0] = 0xdd;
            _ram[1] = 0xe9; // JP (IX)

            WideRegister.IX.SetValueOnProcessor(_cpu, 0x49ed);

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(8));
        }
    }
}