using NUnit.Framework;
using System.Collections.Generic;

namespace Z80.Tests
{
    public class InstructionTests
    {
        private Z80Cpu _cpu;

        [OneTimeSetUp]
        public void InitialSetup()
        {
            _cpu = new Z80Cpu();
            _cpu.Initialize();
        }

        [SetUp]
        public void Setup()
        {
            _cpu.Reset();
        }


        public static IEnumerable<object[]> GenerateCycleCountParams()
        {
            var generated = new List<object[]>();

            generated.Add(new object[] { 0x0, 4 });

            #region 8 Bit load instructions

            generated.Add(new object[] { 0x02, 7 }); // LD (BC), A
            generated.Add(new object[] { 0x0a, 7}); // LD A, (BC)
            generated.Add(new object[] { 0x12, 7 }); // LD (DE), A
            generated.Add(new object[] { 0x1a, 7}); // LD A, (DE)

            generated.Add(new object[] { 0x32, 13 }); // LD (nn), A

            generated.Add(new object[] { 0x3a, 13 }); // LD A, (nn)
            generated.Add(new object[] { 0x36, 10 }); // LD (HL), n

            // Generate all the LD r, n instructions
            for (int i = 0x6; i <= 0x2e; i += 8)
            {
                generated.Add(new object[] { i, 7 });
            }
            generated.Add(new object[] { 0x3e, 7 });

            // Generate all the 8 bit Reg->Reg loads
            for (int i = 0x40; i <= 0x7f; i++)
            {
                if (i == 0x70)
                {
                    i = 0x78; // Skip over destination (HL) instructions
                }

                var lowNibble = i & 0xf;
                if (lowNibble != 0x6 && lowNibble != 0xe) // Excludes instructions with source from (HL) as they are in this range
                {
                    generated.Add(new object[] { i, 4 });
                }
                else if (lowNibble == 0x6 || lowNibble == 0xe)
                {
                    generated.Add(new object[] { i, 7 });
                }
            }

            // Generate the LD (HL), r cycles
            for (int i = 0x70; i <= 0x75; i++)
            {
                generated.Add(new object[] { i, 7 });
            }
            generated.Add(new object[] { 0x77, 7 });

            
            generated.Add(new object[] { 0xdd36, 19 }); // LD (IX+d), n
            generated.Add(new object[] { 0xfd36, 19 }); // LD (IY+d), n


            // Generate all the LD r, (IX+d) and (IY+D) instructions
            for (int i = 0xdd46; i <= 0xdd7e; i += 8)
            {
                if (i != 0xdd76)
                {
                    generated.Add(new object[] { i, 19 });
                    generated.Add(new object[] { i + 0x2000, 19 });
                }
            }

            // Generate the LD (IX+d), r and LD (IY+d), r instructions
            for (int i = 0xdd70; i <= 0xdd77; i++)
            {
                if (i != 0xdd76)
                {
                    generated.Add(new object[] { i, 19 });
                    generated.Add(new object[] { i + 0x2000, 19 });
                }
            }

            generated.Add(new object[] { 0xed47, 9 }); // LD I, A
            generated.Add(new object[] { 0xed4f, 9 }); // LD R, A           

            generated.Add(new object[] { 0xed57, 9 }); // LD A, I
            generated.Add(new object[] { 0xed5f, 9 }); // LD A, R
            #endregion

            return generated;
        }

        [TestCaseSource("GenerateCycleCountParams")]
        public void CycleCounts(int opcode, int expectedCycleCount)
        {
            // Arrange
            // Account for the fetch cycles which are tested elsewhere
            var tCycleCount = 4;
            if (opcode > 0xff)
            {
                tCycleCount = 8;
            }

            var instruction = _cpu.instructions[opcode];

            // Act
            while (!instruction.IsComplete)
            {
                instruction.Clock();
                tCycleCount++;
            }

            // Assert
            Assert.That(tCycleCount, Is.EqualTo(expectedCycleCount));
        }

    }
}