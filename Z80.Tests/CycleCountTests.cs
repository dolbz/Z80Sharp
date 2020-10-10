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


            for (int i = 0xa0; i < 0xa8; i++) {
                if (i == 0xa6) {
                    generated.Add(new object[] { i, 7}); // AND A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // AND A, r
                }
            }

            generated.Add(new object[] { 0xdda6, 19 }); // AND A, (IX+d)
            generated.Add(new object[] { 0xfda6, 19 }); // AND A, (IY+d)

            generated.Add(new object[] { 0xe6, 7 }); // AND A, n


            for (int i = 0xa8; i < 0xb0; i++) {
                if (i == 0xae) {
                    generated.Add(new object[] { i, 7}); // XOR A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // XOR A, r
                }
            }

            generated.Add(new object[] { 0xddae, 19 }); // XOR A, (IX+d)
            generated.Add(new object[] { 0xfdae, 19 }); // XOR A, (IY+d)

            generated.Add(new object[] { 0xee, 7 }); // XOR A, n


            for (int i = 0xb0; i < 0xb8; i++) {
                if (i == 0xb6) {
                    generated.Add(new object[] { i, 7}); // OR A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // OR A, r
                }
            }

            generated.Add(new object[] { 0xddb6, 19 }); // OR A, (IX+d)
            generated.Add(new object[] { 0xfdb6, 19 }); // OR A, (IY+d)

            generated.Add(new object[] { 0xf6, 7 }); // OR A, n

            
            for (int i = 0xb8; i < 0xc0; i++) {
                if (i == 0xbe) {
                    generated.Add(new object[] { i, 7}); // CP A, (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // CP A, r
                }
            }

            generated.Add(new object[] { 0xddbe, 19 }); // CP A, (IX+d)
            generated.Add(new object[] { 0xfdbe, 19 }); // CP A, (IY+d)

            generated.Add(new object[] { 0xfe, 7 }); // CP A, n

            for (int i = 0x04; i <= 0x3c; i += 8) {
                if (i == 0x34) {
                    generated.Add(new object[] { i, 11 }); // INC (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // INC r
                }
            }

            generated.Add(new object[] { 0xdd34, 23 }); // INC (IX+d)
            generated.Add(new object[] { 0xfd34, 23 }); // INC (IY+d)

            for (int i = 0x05; i <= 0x3d; i += 8) {
                if (i == 0x35) {
                    generated.Add(new object[] { i, 11 }); // DEC (HL)
                } else {
                    generated.Add(new object[] { i, 4 }); // DEC r
                }
            }

            generated.Add(new object[] { 0xdd35, 23 }); // DEC (IX+d)
            generated.Add(new object[] { 0xfd35, 23 }); // DEC (IY+d)

            #endregion

            #region General purpose AF operations

            generated.Add(new object[] { 0x27, 4 }); // DAA
            generated.Add(new object[] { 0x2f, 4 }); // CPL
            generated.Add(new object[] { 0xed44, 8 }); // NEG
            generated.Add(new object[] { 0x3f, 4 }); // CCF
            generated.Add(new object[] { 0x37, 4 }); // SCF

            #endregion

            #region 16-bit arithmetic operations

            generated.Add(new object[] { 0x09, 11 }); // ADD HL, BC
            generated.Add(new object[] { 0x19, 11 }); // ADD HL, DE
            generated.Add(new object[] { 0x29, 11 }); // ADD HL, HL
            generated.Add(new object[] { 0x39, 11 }); // ADD HL, SP

            generated.Add(new object[] { 0xdd09, 15 }); // ADD IX, BC
            generated.Add(new object[] { 0xdd19, 15 }); // ADD IX, DE
            generated.Add(new object[] { 0xdd39, 15 }); // ADD IX, SP
            generated.Add(new object[] { 0xdd29, 15 }); // ADD IX, IX

            generated.Add(new object[] { 0xfd09, 15 }); // ADD IY, BC
            generated.Add(new object[] { 0xfd19, 15 }); // ADD IY, DE
            generated.Add(new object[] { 0xfd39, 15 }); // ADD IY, SP
            generated.Add(new object[] { 0xfd29, 15 }); // ADD IY, IY

            generated.Add(new object[] { 0xed4a, 15 }); // ADC HL, BC
            generated.Add(new object[] { 0xed5a, 15 }); // ADC HL, DE
            generated.Add(new object[] { 0xed6a, 15 }); // ADC HL, HL
            generated.Add(new object[] { 0xed7a, 15 }); // ADC HL, SP

            generated.Add(new object[] { 0xed42, 15 }); // SBC HL, BC
            generated.Add(new object[] { 0xed52, 15 }); // SBC HL, DE
            generated.Add(new object[] { 0xed62, 15 }); // SBC HL, HL
            generated.Add(new object[] { 0xed72, 15 }); // SBC HL, SP

            generated.Add(new object[] { 0x03, 6 }); // INC BC
            generated.Add(new object[] { 0x13, 6 }); // INC DE
            generated.Add(new object[] { 0x23, 6 }); // INC HL
            generated.Add(new object[] { 0x33, 6 }); // INC SP
            generated.Add(new object[] { 0xdd23, 10 }); // INC IX
            generated.Add(new object[] { 0xfd23, 10 }); // INC IY

            generated.Add(new object[] { 0x0b, 6 }); // DEC BC
            generated.Add(new object[] { 0x1b, 6 }); // DEC DE
            generated.Add(new object[] { 0x2b, 6 }); // DEC HL
            generated.Add(new object[] { 0x3b, 6 }); // DEC SP
            generated.Add(new object[] { 0xdd2b, 10 }); // DEC IX
            generated.Add(new object[] { 0xfd2b, 10 }); // DEC IY

            #endregion

            #region Rotates and Shifts

            generated.Add(new object[] { 0x07, 4 }); // RLCA
            generated.Add(new object[] { 0x0f, 4 }); // RRCA
            generated.Add(new object[] { 0x17, 4 }); // RLA
            generated.Add(new object[] { 0x1f, 4 }); // RRA

            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb00 + i, 15 }); // RLC (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb00 + i, 8 }); // RLC r
                }
            }

            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb08 + i, 15 }); // RRC (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb08 + i, 8 }); // RRC r
                }
            }

            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb10 + i, 15 }); // RL (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb10 + i, 8 }); // RL r
                }
            }

            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb18 + i, 15 }); // RR (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb18 + i, 8 }); // RR r
                }
            }
            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb20 + i, 15 }); // SLA (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb20 + i, 8 }); // SLA r
                }
            }
            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb28 + i, 15 }); // SRA (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb28 + i, 8 }); // SRA r
                }
            }
            for(int i = 0; i < 8; i++) 
            {
                if (i == 6) 
                {
                    generated.Add(new object[] { 0xcb38 + i, 15 }); // SRL (HL)
                } 
                else 
                {
                    generated.Add(new object[] { 0xcb38 + i, 8 }); // SRL r
                }
            }

            // generated.Add(new object[] { 0xddcb, 23 }); // RLC (IX+d) // TODO how to test these?
            
            generated.Add(new object[] { 0xed6f, 18 });
            generated.Add(new object[] { 0xed67, 18 });

            for (int i = 0xcb47; i < 0xcbff; i++) {
                if (((i & 0xf) == 0x6) || ((i & 0xf) == 0xe)) {
                    generated.Add(new object[] { i, i < 0xcb80 ? 12 : 15 }); // 12 for BIT instructions, 15 for SET/RST
                } else {
                    generated.Add(new object[] { i, 8 }); // All register targeted BIT, SET, RST instructions
                }
            }

            // TODO indexed BIT,SET,RST

            // Jump and call instructions have cycle count tests in with their other unit tests as the cycle
            // count varies depending on the outcome

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
            instruction.StartExecution();

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