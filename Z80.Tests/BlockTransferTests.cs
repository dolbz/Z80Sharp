using System.Runtime.Serialization;
using NUnit.Framework;

namespace Z80.Tests
{
    public class BlockTransferTests : CpuRunTestBase
    {

        [Test]
        public void LoadAndIncrementTest()
        {
            // Arrange
            ushort sourceAddress = 0xf1bc;
            ushort destinationAddress = 0x42d4;

            _ram[sourceAddress] = 0xf7;
            WideRegister.HL.SetValueOnProcessor(_cpu, sourceAddress);
            WideRegister.DE.SetValueOnProcessor(_cpu, destinationAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, 10);

            _ram[0] = 0xed;
            _ram[1] = 0xa0;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[destinationAddress], Is.EqualTo(_ram[sourceAddress]));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(9));
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(sourceAddress + 1));
            Assert.That(WideRegister.DE.GetValue(_cpu), Is.EqualTo(destinationAddress + 1));
        }

        [Test]
        public void LoadAndDecrementTest()
        {
            // Arrange
            var sourceAddress = 0x27b3;
            var destinationAddress = 0x5e22;

            _ram[sourceAddress] = 0xf7;
            WideRegister.HL.SetValueOnProcessor(_cpu, (ushort)sourceAddress);
            WideRegister.DE.SetValueOnProcessor(_cpu, (ushort)destinationAddress);
            WideRegister.BC.SetValueOnProcessor(_cpu, 12);

            _ram[0] = 0xed;
            _ram[1] = 0xa8;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_ram[destinationAddress], Is.EqualTo(_ram[sourceAddress]));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(11));
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(sourceAddress - 1));
            Assert.That(WideRegister.DE.GetValue(_cpu), Is.EqualTo(destinationAddress - 1));
        }

        // TODO test cases for inc and repeat variants?
        [Test]
        public void LoadAndDecrementPvFlagResetTest() {
            // Arrange
            var expectedFlags = _cpu.Flags; // Record the original state and manipulate just those we expect to change

            _cpu.Flags |= Z80Flags.AddSubtract_N; // Set N
            _cpu.Flags |= Z80Flags.HalfCarry_H; // Set H

            _ram[0] = 0xed;
            _ram[1] = 0xa8;

            WideRegister.BC.SetValueOnProcessor(_cpu, 2);

            // Act
            RunUntil(3);

            // Assert
            expectedFlags &= ~Z80Flags.AddSubtract_N; // Reset N
            expectedFlags &= ~Z80Flags.HalfCarry_H; // Reset H
            
            expectedFlags &= ~Z80Flags.ParityOverflow_PV; // Reset PV as BC remains > 0
            
            Assert.That(_cpu.Flags, Is.EqualTo(expectedFlags));
        }

        // TODO test cases for inc and repeat variants?
        [Test]
        public void LoadAndDecrementPvFlagSetTest() {
            // Arrange
            var expectedFlags = _cpu.Flags; // Record the original state and manipulate just those we expect to change

            _cpu.Flags &= ~Z80Flags.ParityOverflow_PV; // Reset PV

            _ram[0] = 0xed;
            _ram[1] = 0xa8;

            WideRegister.BC.SetValueOnProcessor(_cpu, 1);

            // Act
            RunUntil(3);

            // Assert
            expectedFlags |= Z80Flags.ParityOverflow_PV; // Set PV as BC = 0
            
            Assert.That(_cpu.Flags, Is.EqualTo(expectedFlags));
        }

        [Test]
        public void LoadAndIncrementRepeatTest()
        {
            // Arrange
            var data = new byte[] { 0x84, 0xb1, 0x32, 0xee, 0x4c };

            _ram[0] = 0xed;
            _ram[1] = 0xb0;

            _ram[0x69a1] = data[0];
            _ram[0x69a2] = data[1];
            _ram[0x69a3] = data[2];
            _ram[0x69a4] = data[3];
            _ram[0x69a5] = data[4];

            // LDIR Moves BC bytes from HL -> DE
            WideRegister.BC.SetValueOnProcessor(_cpu, (ushort)data.Length);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x69a1);  
            WideRegister.DE.SetValueOnProcessor(_cpu, 0xd00d);

            // Act
            RunUntil(3);

            // Assert
            for (int i = 0; i < data.Length; i++) {
                Assert.That(_ram[0xd00d + i], Is.EqualTo(_ram[0x69a1 + i]));
            }
        }

        [Test]
        public void LoadAndDecrementRepeatTest()
        {
            // Arrange
            var data = new byte[] { 0x84, 0xb1, 0x32, 0xee, 0x4c };

            _ram[0] = 0xed;
            _ram[1] = 0xb8;

            _ram[0x69a1] = data[4];
            _ram[0x69a2] = data[3];
            _ram[0x69a3] = data[2];
            _ram[0x69a4] = data[1];
            _ram[0x69a5] = data[0];

            // LDIR Moves BC bytes from HL -> DE
            WideRegister.BC.SetValueOnProcessor(_cpu, (ushort)data.Length);
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x69a5);  
            WideRegister.DE.SetValueOnProcessor(_cpu, 0xd011);

            // Act
            RunUntil(3);

            // Assert
            for (int i = 0; i < data.Length; i++) {
                Assert.That(_ram[0xd011 - i], Is.EqualTo(_ram[0x69a5 - i]));
            }
        }
    }
}