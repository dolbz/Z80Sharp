using System.Diagnostics;
using System;

namespace Z80
{

    [Flags]
    public enum Z80Flags : byte
    {
        Sign = 128,
        Zero = 64,
        HalfCarry = 16,
        ParityOverflow = 4,
        Subtract = 2,
        Carry = 1
    }

    public enum Register
    {
        A,
        Flags,
        A_,
        Flags_,
        B,
        C,
        BC,
        D,
        E,
        DE,
        H,
        L,
        HL,
        B_,
        C_,
        BC_,
        D_,
        E_,
        DE_,
        H_,
        L_,
        HL_,
        SP,
        IX,
        IY
    }

    public class Z80Cpu
    {
        private static IInstruction[] instructions = new IInstruction[65536];
        internal ushort Opcode;

        #region General Purpose Registers
        public byte A;
        public Z80Flags Flags = (Z80Flags)0b01010101;
        public byte A_;
        public byte Flags_;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public byte B_;
        public byte C_;
        public byte D_;
        public byte E_;
        public byte H_;
        public byte L_;

        #endregion

        # region Special Purpose Registers

        public byte I;
        public byte R;
        public ushort IX;
        public ushort IY;
        public ushort SP;
        public ushort PC;

        # endregion

        public ushort Address;
        public byte Data;

        public bool MREQ;

        public bool IORQ;

        public bool RD;

        public bool WR;
        
        public int TotalTCycles { get; private set; } = 0;

        private M1Cycle _fetchCycle;
        private IInstruction _currentInstruction;
        public void Clock()
        {
            if (!_fetchCycle.IsComplete) {
                _fetchCycle.Clock();
            } 
            if (_fetchCycle.IsComplete && _currentInstruction == null) {
                Console.WriteLine("Fetch cycle complete");

                var instruction = instructions[Opcode];
                
                if (instruction != null) {
                    _currentInstruction = instruction;
                    _currentInstruction.Reset();
                    _currentInstruction.StartExecution();
                    if (_currentInstruction.IsComplete) {
                        Opcode = 0x0;
                        _fetchCycle.Reset();
                    }
                } else if (Opcode <= 0xFF) {
                    Opcode <<= 8;
                    _fetchCycle.Reset();
                } else {
                    Console.WriteLine("Unknown instruction");
                    Opcode = 0x0;
                    _fetchCycle.Reset();
                }
            } else if (_currentInstruction != null) {
                _currentInstruction.Clock();

                if (_currentInstruction.IsComplete) {
                    _currentInstruction = null;
                    _fetchCycle.Reset();
                }
            }
            TotalTCycles++;
        }

        public void Reset() {
            // RESET forces the program counter to zero and initializes the CPU. The CPU initialization includes:
            // 1) Disable the interrupt enable flip-flop 
            // 2) Set Register I = OOH
            // 3) Set Register R = OOH
            // 4) Set Interrupt Mode 0

            PC = 0x0;
            I = 0x0;
            R = 0x0;
            // TODO steps 1 and 4

            Initialize();

            _fetchCycle.Reset();
        }

        public void Initialize() {
            _fetchCycle = new M1Cycle(this);

            // Load instructions
            instructions[0x0] = new NOP();
            instructions[0x3a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ExtendedPointerOperand(this));
            instructions[0x3e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ImmediateOperand());
            instructions[0x7a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this,Register.D)); // LD A, D 
            instructions[0x7f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.A)); // LD A, A (4 T cycles)
            //instructions[0xf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, Register.SP),new RegAddrMode16Bit(this, Register.HL)); // LD SP, HL (6 T cycles)

            instructions[0xdd7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new IndexedRead(this, Register.IX)); // LD A, (IX+d)
        }
    }
}
