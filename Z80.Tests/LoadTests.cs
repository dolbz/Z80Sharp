using NUnit.Framework;

namespace Z80.Tests
{
    public class Load8BitTests : CpuRunTestBase
    {
        [Test]
        public void LoadRegisterToRegister()
        {
            // Arrange
            // TODO these three could be method parameters and generalised to test all the Reg->Reg instructions
            Register destination = Register.A;
            Register source = Register.B;
            byte opcode = 0x78;

            byte value = 0xad;
            source.SetValueOnProcessor(_cpu, value);
            _ram[0] = opcode;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(destination.GetValue(_cpu), Is.EqualTo(value));
        }

        [Test]
        public void LoadRegisterPointerToRegister()
        {
            // Arrange
            // Again these 3 can be parameterized
            Register destination = Register.A;
            WideRegister source = WideRegister.HL;
            byte opcode = 0x7e;

            byte value = 0x1f;
            ushort pointer = 0x0bad;
            source.SetValueOnProcessor(_cpu, pointer);
            _ram[0] = opcode;
            _ram[pointer] = value;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(destination.GetValue(_cpu), Is.EqualTo(value));
        }

        [Test]
        public void LoadIndexedPointerToRegister()
        {
            // Arrange
            byte loadValue = 0xc9;
            var destination = Register.B;
            var indexRegister = WideRegister.IX;
            ushort pointer = 0xf00d;
            byte instructionByte1 = 0xdd;
            byte instructionByte2 = 0x46; // register B
            byte instructionByte3 = 0xfe; // -2 2's complement.


            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;
            _ram[2] = instructionByte3;
            _ram[pointer-2] = loadValue;
            indexRegister.SetValueOnProcessor(_cpu, pointer);
            

            // Act
            RunUntil(4);

            // Assert
            Assert.That(destination.GetValue(_cpu), Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadImmediateExtendedPointerToA()
        {
            // Arrange
            byte loadValue = 0x72;
            byte instructionByte1 = 0x3a;
            byte instructionByte2 = 0xee;
            byte instructionByte3 = 0xbb;

            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;
            _ram[2] = instructionByte3;
            _ram[0xbbee] = loadValue;


            // Act
            RunUntil(4);

            // Assert
            Assert.That(_cpu.A, Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadImmediateToPointer()
        {
            // Arrange
            byte loadValue = 0x8c;
            var destination = Register.C;
            byte instructionByte1 = 0x0e;
            byte instructionByte2 = loadValue;

            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;

            // Act
            RunUntil(3);

            // Assert
            Assert.That(destination.GetValue(_cpu), Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadRegisterToRegisterPointer()
        {
            // Arrange
            WideRegister pointerRegister = WideRegister.BC;
            Register source = Register.A;
            byte opcode = 0x02;

            byte loadValue = 0xe3;
            ushort pointer = 0xdead;
            
            pointerRegister.SetValueOnProcessor(_cpu, pointer);
            source.SetValueOnProcessor(_cpu, loadValue);
            _ram[0] = opcode;

            // Act
            RunUntil(2);

            // Assert
            Assert.That(_ram[pointer], Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadRegisterToIndexedPointer()
        {
            // Arrange
            WideRegister indexRegister = WideRegister.IY;
            Register source = Register.E;
            byte instructionByte1 = 0xfd;
            byte instructionByte2 = 0x73;

            byte loadValue = 0x28;
            ushort pointer = 0xdeaf;
            
            indexRegister.SetValueOnProcessor(_cpu, pointer);
            source.SetValueOnProcessor(_cpu, loadValue);
            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;
            _ram[2] = 0x10;

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_ram[pointer + 0x10], Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadImmediateToIndexedPointer()
        {
            // Arrange
            WideRegister indexRegister = WideRegister.IX;
            byte instructionByte1 = 0xdd;
            byte instructionByte2 = 0x36;

            byte loadValue = 0x86;
            ushort pointer = 0xb00b;
            
            indexRegister.SetValueOnProcessor(_cpu, pointer);
            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;
            _ram[2] = 0xf;
            _ram[3] = loadValue;

            // Act
            RunUntil(5);

            // Assert
            Assert.That(_ram[pointer + 0xf], Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadAToImmediateExtendedPointer()
        {
            // Arrange
            byte instructionByte1 = 0x32;
            byte instructionByte2 = 0xc4;
            byte instructionByte3 = 0x52;

            byte loadValue = 0x90;
            
            _cpu.A = loadValue;
            _ram[0] = instructionByte1;
            _ram[1] = instructionByte2;
            _ram[2] = instructionByte3;

            // Act
            RunUntil(4);

            // Assert
            Assert.That(_ram[0x52c4], Is.EqualTo(loadValue));
        }
    }

    public class Load16BitTests : CpuRunTestBase { 

        [Test]
        public void LoadRegisterToRegister()
        {
            // Arrange
            WideRegister.HL.SetValueOnProcessor(_cpu, 0x4cf3);
            _ram[0] = 0xf9; // LD SP, HL

            // Act
            RunUntil(3);

            // Assert
            Assert.That(WideRegister.SP.GetValue(_cpu), Is.EqualTo(0x4cf3));
        }

        [Test]
        public void LoadImmediateExtendedPointerToWideRegister(){
            // Arrange
            var targetRegister = WideRegister.DE;
            ushort loadValue = 0x1b40;

            _ram[0] = 0xed; // LD DE, (nn)
            _ram[1] = 0x5b;
            _ram[2] = 0xe5;
            _ram[3] = 0xf0;
            _ram[0xf0e5] = (byte)(loadValue & 0xff);
            _ram[0xf0e6] = (byte)(loadValue >> 8);

            // Act
            RunUntil(5);

            // Assert
            Assert.That(targetRegister.GetValue(_cpu), Is.EqualTo(loadValue));
        }

        [Test]
        public void LoadImmediateExtendedValueToWideRegister(){
            // Arrange
            var targetRegister = WideRegister.BC;

            _ram[0] = 0x01; // LD BC, nn
            _ram[1] = 0x5b;
            _ram[2] = 0xe5;

            // Act
            RunUntil(4);

            // Assert
            Assert.That(targetRegister.GetValue(_cpu), Is.EqualTo(0xe55b));
        }

        [Test]
        public void LoadRegisterToImmediateExtendedPointer(){
            // Arrange
            var sourceRegister = WideRegister.IX;
            ushort pointer = 0x62f1;
            var msb = 0x91;
            var lsb = 0x2d;

            sourceRegister.SetValueOnProcessor(_cpu, (ushort)((msb << 8) | lsb));

            _ram[0] = 0xdd; // LD (nn), IX
            _ram[1] = 0x22;
            _ram[2] = (byte)(pointer & 0xff);
            _ram[3] = (byte)(pointer >> 8);

            // Act
            RunUntil(5);

            // Assert
            Assert.That(_ram[pointer], Is.EqualTo(lsb));
            Assert.That(_ram[pointer+1], Is.EqualTo(msb));
        }
    }
}