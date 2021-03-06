//  
// Copyright (c) 2021, Nathan Randle <nrandle@dolbz.com>. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using NUnit.Framework;

namespace Z80.Tests {
    public class DecimalAdjustAccumulatorTests : CpuRunTestBase {


        // Addition cases
        [TestCase(false, false, false, 0x50, 0x50, false)] // No correction needed for value in accumulator does nothing
        [TestCase(false, false, false, 0xb0, 0x10, true)] // Upper nibble > 10 but not overflowing corrects upper nibble and sets carry flag out
        [TestCase(false, false, false, 0x0e, 0x14, false)] // Lower nibble > 10 but not overflowing corrects lower nibble and adds 1 to upper nibble
        [TestCase(false, false, true, 0x21, 0x27, false)] // Half carry flag set corrects lower nibble
        [TestCase(false, true, false, 0x23, 0x83, true)] // Carry flag set corrects upper nibble
        [TestCase(false, true, true, 0x32, 0x98, true)] // Both carry flags set corrects both nibbles

        // Subtraction cases
        [TestCase(true, false, false, 0x62, 0x62, false)] // No correction needed for value in accumulator does nothing
        [TestCase(true, false, true, 0x6e, 0x68, false)] // Half carry flag set corrects lower nibble
        [TestCase(true, true, false, 0xd9, 0x79, true)] // Carry flag set corrects upper nibble
        [TestCase(true, true, true, 0xc7, 0x61, true)] // Both carry flags set corrects both nibbles
        public void TestDecimalAdjustment(
            bool subtractFlag,
            bool carryInFlag, 
            bool halfCarryFlag, 
            byte accumulatorValue, 
            byte correctedValue,
            bool carryOut) 
        {
            // Arrange
            _ram[0] = 0x27;

            Z80Flags.HalfCarry_H.SetOrReset(_cpu, halfCarryFlag);
            Z80Flags.Carry_C.SetOrReset(_cpu, carryInFlag);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, subtractFlag);
            Register.A.SetValueOnProcessor(_cpu, accumulatorValue);

            // Act
            RunUntil(3);

            // Assert
            Assert.That(Register.A.GetValue(_cpu), Is.EqualTo(correctedValue));
            Assert.That(_cpu.Flags.HasFlag(Z80Flags.Carry_C), Is.EqualTo(carryOut));
        }
    }
}