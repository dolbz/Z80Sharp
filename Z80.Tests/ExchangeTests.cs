
using NUnit.Framework;

namespace Z80.Tests
{
    public class ExchangeTests : CpuRunTestBase
    {
        [Test]
        public void ExchangeAF() {
            // Arrange
            ushort afRegister = 0x694e;
            ushort altAfRegister = 0x70e1;
            WideRegister.AF.SetValueOnProcessor(_cpu, afRegister);
            _cpu.AF_ = altAfRegister;

            _ram[0] = 0x08;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.AF_, Is.EqualTo(afRegister));
            Assert.That(WideRegister.AF.GetValue(_cpu), Is.EqualTo(altAfRegister));
        }

        [Test]
        public void ExchangeDEHL() {
            // Arrange
            ushort deRegister = 0x994e;
            ushort hlRegister = 0x621b;
            WideRegister.DE.SetValueOnProcessor(_cpu, deRegister);
            WideRegister.HL.SetValueOnProcessor(_cpu, hlRegister);

            _ram[0] = 0xeb;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.DE.GetValue(_cpu), Is.EqualTo(hlRegister));
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(deRegister));
        }

        [Test]
        public void ExchangeOthers() {
            // Arrange
            ushort bcReg = 0xd5c3;
            ushort altBcReg = 0x91e2;
            WideRegister.BC.SetValueOnProcessor(_cpu, bcReg);
            WideRegister.BC_.SetValueOnProcessor(_cpu, altBcReg);

            ushort deReg = 0x102e;
            ushort altDeReg = 0x311d;
            WideRegister.DE.SetValueOnProcessor(_cpu, deReg);
            WideRegister.DE_.SetValueOnProcessor(_cpu, altDeReg);

            ushort hlReg = 0x7511;
            ushort altHlReg = 0x82f7;
            WideRegister.HL.SetValueOnProcessor(_cpu, hlReg);
            WideRegister.HL_.SetValueOnProcessor(_cpu, altHlReg);

            _ram[0] = 0xd9;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(_cpu.BC_, Is.EqualTo(bcReg));
            Assert.That(WideRegister.BC.GetValue(_cpu), Is.EqualTo(altBcReg));

            Assert.That(_cpu.DE_, Is.EqualTo(deReg));
            Assert.That(WideRegister.DE.GetValue(_cpu), Is.EqualTo(altDeReg));
                        
            Assert.That(_cpu.HL_, Is.EqualTo(hlReg));
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(altHlReg));
        }

        [Test]
        public void ExchangeHL_StackPointer() {
            // Arrange
            ushort valueOnStack = 0x25c2;
            ushort spValue = 0xdd26;
            ushort hlRegister = 0x815d;
            WideRegister.HL.SetValueOnProcessor(_cpu, hlRegister);
            _cpu.SP = spValue;

            _ram[0] = 0xe3;
            _ram[spValue] = (byte)(valueOnStack & 0xff);
            _ram[spValue+1] = (byte)(valueOnStack >> 8);

            // Act
            RunUntil(5);

            // Assert
            Assert.That(WideRegister.HL.GetValue(_cpu), Is.EqualTo(valueOnStack));
            Assert.That(_ram[spValue], Is.EqualTo(hlRegister & 0xff));
            Assert.That(_ram[spValue + 1], Is.EqualTo(hlRegister >> 8));
        }
    }
}