//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests.ReturnCallReturn {
    public class ReturnTests : CpuRunTestBase {
        
        [Test]
        public void ReturnUnconditional() {
            // Arrange
            _ram[0x331d] = 0xc9; // RET
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void ReturnFromInterrupt() {
            // Arrange
            _ram[0x331d] = 0xed; // RETI
            _ram[0x331e] = 0x4d;
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(14));
        }

        [Test]
        public void ReturnFromNonMaskableInterrupt() {
            // Arrange
            _ram[0x331d] = 0xed; // RETN
            _ram[0x331e] = 0x45;
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            _cpu.IFF1 = false;
            _cpu.IFF2 = true;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(14));
            Assert.That(_cpu.IFF1, Is.True);
        }

        [Test]
        public void ReturnIfCarry_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xd8; // RET C
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnIfCarry_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xd8; // RET C
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnIfNonCarry_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xd0; // RET NC
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;


            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnIfNonCarry_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xd0; // RET NC
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnIfZero_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xc8; // RET Z
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnIfZero_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xc8; // RET Z
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnIfNonZero_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xc0; // RET NZ
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnIfNonZero_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xc0; // RET NZ
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnParityEven_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xe8; // RET PE
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnParityEven_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xe8; // RET PE
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnParityOdd_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xe0; // RET PO
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnParityOdd_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xe0; // RET PO
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnSignNegative_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xf8; // RET M
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;


            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnSignNegative_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xf8; // RET M
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

        [Test]
        public void ReturnSignPositive_ShouldReturn() {
            // Arrange
            _ram[0x331d] = 0xf0; // RET P
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2f));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }

        [Test]
        public void ReturnSignPositive_ShouldNotReturn() {
            // Arrange
            _ram[0x331d] = 0xf0; // RET P
            _ram[0xff2d] = 0xed;
            _ram[0xff2e] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;
            _cpu.PC = 0x331d;
            _cpu.SP = 0xff2d;

            // Act
            RunUntil(0x331e);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x331e));
            Assert.That(_cpu.SP, Is.EqualTo(0xff2d));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(5));
        }

    }
}