using NUnit.Framework;
using System.Collections.Generic;

namespace Z80.Tests
{
    public class CycleCountTests
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

            #region 8 bit load instructions

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

            #region 16 bit load instructions

            // Load immediate
            generated.Add(new object[] { 0x01, 10 });
            generated.Add(new object[] { 0x11, 10 });
            generated.Add(new object[] { 0x21, 10 });
            generated.Add(new object[] { 0x31, 10 });
            generated.Add(new object[] { 0xdd21, 14 });
            generated.Add(new object[] { 0xfd21, 14 });

            // Load immmediate pointer
            generated.Add(new object[] { 0xed4b, 20 });
            generated.Add(new object[] { 0xed5b, 20 });
            generated.Add(new object[] { 0x2a, 16 });
            generated.Add(new object[] { 0xed7b, 20 });
            generated.Add(new object[] { 0xdd2a, 20 });
            generated.Add(new object[] { 0xfd2a, 20 });

            // Load register -> SP
            generated.Add(new object[] { 0xf9, 6 });
            generated.Add(new object[] { 0xddf9, 10 });
            generated.Add(new object[] { 0xfdf9, 10 });

            // Load register -> immediate pointer
            generated.Add(new object[] { 0xed43, 20 });
            generated.Add(new object[] { 0xed53, 20 });
            generated.Add(new object[] { 0x22, 16 });
            generated.Add(new object[] { 0xed73, 20 });
            generated.Add(new object[] { 0xdd22, 20 });
            generated.Add(new object[] { 0xfd22, 20 });

            #endregion

            #region Push instructions

            generated.Add(new object[] { 0xc5, 11 });
            generated.Add(new object[] { 0xd5, 11 });
            generated.Add(new object[] { 0xe5, 11 });
            generated.Add(new object[] { 0xf5, 11 });


            generated.Add(new object[] { 0xdde5, 15 });
            generated.Add(new object[] { 0xfde5, 15 });

            #endregion

            #region pop instructions

            generated.Add(new object[] { 0xc1, 10 });
            generated.Add(new object[] { 0xd1, 10 });
            generated.Add(new object[] { 0xe1, 10 });
            generated.Add(new object[] { 0xf1, 10 });


            generated.Add(new object[] { 0xdde1, 14 });
            generated.Add(new object[] { 0xfde1, 14 });

            #endregion

            #region exchange instructions

            generated.Add(new object[] { 0x08, 4 });
            generated.Add(new object[] { 0xd9, 4 });
            generated.Add(new object[] { 0xeb, 4 });

            generated.Add(new object[] { 0xe3, 19 });
            generated.Add(new object[] { 0xdde3, 23 });
            generated.Add(new object[] { 0xfde3, 23 });

            #endregion

            #region Block transfer instructions

            // LDIR and LDDR tested separately as the cycle count is variable
            generated.Add(new object[] { 0xeda0, 16 });
            generated.Add(new object[] { 0xeda8, 16 });

            #endregion

            #region Block search instructions

            // CPIR and CPDR tested separartely as the cycle count is variable
            generated.Add(new object[] { 0xeda1, 16 });
            generated.Add(new object[] { 0xeda9, 16 });

            #endregion

            #region 8-bit arithmetic and logic

            for (int i = 0x80; i < 0x88; i++) {
                if (i == 0x86) {
                    generated.Add(new object[] { i, 7}); // ADD A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // ADD A, r
                }
            }

            generated.Add(new object[] { 0xdd86, 19 }); // ADD A, (IX+d)
            generated.Add(new object[] { 0xfd86, 19 }); // ADD A, (IY+d)

            generated.Add(new object[] { 0xc6, 7 }); // ADD A, n


            for (int i = 0x88; i < 0x90; i++) {
                if (i == 0x8e) {
                    generated.Add(new object[] { i, 7}); // ADC A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // ADC A, r
                }
            }

            generated.Add(new object[] { 0xdd8e, 19 }); // ADC A, (IX+d)
            generated.Add(new object[] { 0xfd8e, 19 }); // ADC A, (IY+d)

            generated.Add(new object[] { 0xce, 7 }); // ADC A, n


            for (int i = 0x90; i < 0x98; i++) {
                if (i == 0x96) {
                    generated.Add(new object[] { i, 7}); // SUB A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // SUB A, r
                }
            }

            generated.Add(new object[] { 0xdd96, 19 }); // SUB A, (IX+d)
            generated.Add(new object[] { 0xfd96, 19 }); // SUB A, (IY+d)

            generated.Add(new object[] { 0xd6, 7 }); // SUB A, n


            for (int i = 0x98; i < 0xa0; i++) {
                if (i == 0x9e) {
                    generated.Add(new object[] { i, 7}); // SBC A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // SBC A, r
                }
            }

            generated.Add(new object[] { 0xdd9e, 19 }); // SBC A, (IX+d)
            generated.Add(new object[] { 0xfd9e, 19 }); // SBC A, (IY+d)

            generated.Add(new object[] { 0xde, 7 }); // SBC A, n

            #endregion

            return generated;
        }

        [TestCase(0xedb0)]
        [TestCase(0xedb8)]
        public void LoadIncOrDecRepeatCycleCount(int opcode) {
            WideRegister.BC.SetValueOnProcessor(_cpu, 2);

            CycleCounts(opcode, 21);
            CycleCounts(opcode, 16); // Second time through BC should be 1 going to 0 so only 16 cycles
        }

        [TestCase(0xedb1)]
        [TestCase(0xedb9)]
        public void CompareIncOrDecRepeatCycleCount(int opcode) {
            WideRegister.BC.SetValueOnProcessor(_cpu, 2);

            CycleCounts(opcode, 21);
            CycleCounts(opcode, 16); // Second time through BC should be 1 going to 0 so only 16 cycles
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
            instruction.Reset();

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