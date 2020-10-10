using NUnit.Framework;

namespace Z80.Tests.JumpCallReturn {
    public class JumpRelativeTests : CpuRunTestBase {
        
        [Test]
        public void JumpForwardUnconditional() {
            // Arrange
            _ram[480] = 0x18; // JP $+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpBackwardsUnconditional() {
            // Arrange
            _ram[480] = 0x18; // JP $-5
            unchecked {
                _ram[481] = (byte)-7; // -7 as offset-2 is whats assembled in the binary
            }

            _cpu.PC = 480;

            // Act
            RunUntil(475);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(475));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpRelativeCarry_ShouldJump() {
            // Arrange
            _ram[480] = 0x38; // JP C,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpRelativeCarry_ShouldNotJump() {
            // Arrange
            _ram[480] = 0x38; // JP C,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = 0;
            _cpu.PC = 480;

            // Act
            RunUntil(482);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(482));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(7));
        }

        [Test]
        public void JumpRelativeNonCarry_ShouldJump() {
            // Arrange
            _ram[480] = 0x30; // JP NC,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = 0;
            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpRelativeNonCarry_ShouldNotJump() {
            // Arrange
            _ram[480] = 0x30; // JP NC,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = Z80Flags.Carry_C;
            _cpu.PC = 480;

            // Act
            RunUntil(482);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(482));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(7));
        }

        [Test]
        public void JumpRelativeZero_ShouldJump() {
            // Arrange
            _ram[480] = 0x28; // JP Z,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpRelativeZero_ShouldNotJump() {
            // Arrange
            _ram[480] = 0x28; // JP Z,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = 0;
            _cpu.PC = 480;

            // Act
            RunUntil(482);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(482));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(7));
        }

        [Test]
        public void JumpRelativeNonZero_ShouldJump() {
            // Arrange
            _ram[480] = 0x20; // JP NZ,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = 0;
            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(12));
        }

        [Test]
        public void JumpRelativeNonZero_ShouldNotJump() {
            // Arrange
            _ram[480] = 0x20; // JP NZ,$+5
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.Flags = Z80Flags.Zero_Z;
            _cpu.PC = 480;

            // Act
            RunUntil(482);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(482));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(7));
        }

        [Test]
        public void DecrementBJumpIfNotZero_ShouldNotJump() {
            // Arrange
            _ram[480] = 0x10; // DJNZ, e
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.B = 1;
            _cpu.PC = 480;

            // Act
            RunUntil(482);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(482));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(8));
        }

        [Test]
        public void DecrementBJumpIfNotZero_ShouldJump() {
            // Arrange
            _ram[480] = 0x10; // DJNZ, e
            _ram[481] = 3; // 3 as offset-2 is whats assembled in the binary

            _cpu.B = 5;
            _cpu.PC = 480;

            // Act
            RunUntil(485);

            // Assert
            Assert.That(_cpu.PC, Is.EqualTo(485));
            Assert.That(_cpu.B, Is.EqualTo(4));
            Assert.That(_cpu.TotalTCycles, Is.EqualTo(13));
        }
    }
}