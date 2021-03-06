//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.JumpCallReturn {
        public class RestartTests : CpuRunTestBase {
        
        [TestCase(0xc7, 0)]
        [TestCase(0xcf, 8)]
        [TestCase(0xd7, 16)]
        [TestCase(0xdf, 24)]
        [TestCase(0xe7, 32)]
        [TestCase(0xef, 40)]
        [TestCase(0xf7, 48)]
        [TestCase(0xff, 56)]
        public void RestartX(int opcode, int expectedPc) {
            // Arrange
            _ram[0x1e25] = (byte)opcode; // RST x

            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(expectedPc);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo((ushort)expectedPc));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x26));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(11));
        }
    }
}