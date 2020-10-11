using NUnit.Framework;

namespace Z80.Tests.JumpCallReturn
{
    public class CallTests : CpuRunTestBase {
        
        [Test]
        public void CallUnconditional() {
            // Arrange
            _ram[0x1e25] = 0xcd; // CALL 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallCarry_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xdc; // CALL C, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallCarry_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xdc; // CALL C, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallNonCarry_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xd4; // CALL NC, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallNonCarry_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xd4; // CALL NC, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }
        
        [Test]
        public void CallZero_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xcc; // CALL Z, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallZero_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xcc; // CALL Z, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallNonZero_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xc4; // CALL NZ, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallNonZero_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xc4; // CALL NZ, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallParityEven_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xec; // CALL PE, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallParityEven_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xec; // CALL PE, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallParityOdd_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xe4; // CALL PO, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallParityOdd_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xe4; // CALL PO, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.ParityOverflow_PV;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallSignNeg_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xfc; // CALL M, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallSignNeg_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xfc; // CALL M, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }

        [Test]
        public void CallSignPositive_ShouldJump() {
            // Arrange
            _ram[0x1e25] = 0xf4; // CALL P, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = 0;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x49ed);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x49ed));
            Assert.That(_cpu.SP, Is.EqualTo(0xfffe));
            Assert.That(_ram[0xffff], Is.EqualTo(0x1e));
            Assert.That(_ram[0xfffe], Is.EqualTo(0x28));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(17));
        }

        [Test]
        public void CallSignPositive_ShouldNotJump() {
            // Arrange
            _ram[0x1e25] = 0xf4; // CALL P, 0x49ed
            _ram[0x1e26] = 0xed;
            _ram[0x1e27] = 0x49;

            _cpu.Flags = Z80Flags.Sign_S;
            _cpu.PC = 0x1e25;
            _cpu.SP = 0;

            // Act
            RunUntil(0x1e28);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(0x1e28));
            Assert.That(_cpu.SP, Is.EqualTo(0));
            
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(10));
        }
    }
}