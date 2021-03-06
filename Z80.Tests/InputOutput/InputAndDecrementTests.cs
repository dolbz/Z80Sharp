//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Z80.Tests.InputOutput
{
    public class InputAndDecrementTests : CpuRunTestBase {

        [Test]
        public void InputAndDecrementHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xaa;

            _cpu.B = 0x05;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            AddDataAtIoAddress(0x051d, 0x5a);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x2109));
            Assert.That(_cpu.B, Is.EqualTo(0x04));
            Assert.That(_ram[0x210a], Is.EqualTo(0x5a));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.False);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }

        [Test]
        public void InputAndDecrementRepeatingHappyPath() {
            // Arrange
            _ram[0] = 0xed;
            _ram[1] = 0xba;

            _cpu.B = 0x03;
            _cpu.C = 0x1d;
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x210a);
            _cpu.Flags = Z80Flags.Zero_Z;

            AddDataAtIoAddress(0x031d, 0x5a);
            AddDataAtIoAddress(0x021d, 0x5b);
            AddDataAtIoAddress(0x011d, 0x5c);

            // Act
            RunUntil(2);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(0x2107));
            Assert.That(_cpu.B, Is.EqualTo(0x0));
            Assert.That(_ram[0x210a], Is.EqualTo(0x5a));
            Assert.That(_ram[0x2109], Is.EqualTo(0x5b));
            Assert.That(_ram[0x2108], Is.EqualTo(0x5c));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Zero_Z), Is.True);
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.AddSubtract_N), Is.True);
        }
    } 
}