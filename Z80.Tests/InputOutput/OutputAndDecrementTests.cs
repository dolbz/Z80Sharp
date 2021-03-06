//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests.InputOutput
{
    public class OutputAndDecrementTests : CpuRunTestBase {

        [Test]
        public void OutputAndDecrementHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xab;

            _cpu.B = 0x05;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            _ram[0x210a] = 0x5a;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x2109));
            Assert.That(_cpu.B, Is.EqualTo(0x04));
            Assert.That(DataAtIoAddress(0x041d), Is.EqualTo(0x5a));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }

        [Test]
        public void OuputAndDecrementRepeatingHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xbb;

            _cpu.B = 0x03;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            _ram[0x210a] = 0xf7;
            _ram[0x2109] = 0x94;
            _ram[0x2108] = 0x1b;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x2107));
            Assert.That(_cpu.B, Is.EqualTo(0x0));
            Assert.That(DataAtIoAddress(0x021d), Is.EqualTo(0xf7));
            Assert.That(DataAtIoAddress(0x011d), Is.EqualTo(0x94));
            Assert.That(DataAtIoAddress(0x001d), Is.EqualTo(0x1b));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }
    } 
}