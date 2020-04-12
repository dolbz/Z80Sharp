﻿using System.Threading;
using System.Diagnostics;
using System;

namespace Z80
{

    [Flags]
    public enum Z80Flags : byte
    {
        Sign_S = 128,
        Zero_Z = 64,
        HalfCarry_H = 16,
        ParityOverflow_PV = 4,
        AddSubtract_N = 2,
        Carry_C = 1
    }

    public static class Z80FlagsExtensions {
        public static void SetOrReset(this Z80Flags flag, Z80Cpu cpu, bool value) {
            if (value) {
                cpu.Flags |= flag;
            } else {
                cpu.Flags &= ~flag;
            }
        }
    }

    public enum Register
    {
        A,
        Flags,
        B,
        C,
        D,
        E,
        H,
        L,
        I,
        R
    }

    public enum WideRegister
    {
        None = 0,
        AF,
        AF_,
        BC,
        BC_,
        DE,
        DE_,
        HL,
        HL_,
        SP,
        IX,
        IY
    }

    public static class RegisterExtension
    {
        public static void SetValueOnProcessor(this Register register, Z80Cpu cpu, byte value)
        {
            switch (register)
            {
                case Register.A:
                    cpu.A = value;
                    break;
                case Register.B:
                    cpu.B = value;
                    break;
                case Register.C:
                    cpu.C = value;
                    break;
                case Register.D:
                    cpu.D = value;
                    break;
                case Register.E:
                    cpu.E = value;
                    break;
                case Register.H:
                    cpu.H = value;
                    break;
                case Register.I:
                    cpu.I = value;
                    break;
                case Register.L:
                    cpu.L = value;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static byte GetValue(this Register register, Z80Cpu cpu)
        {
            switch (register)
            {
                case Register.A:
                    return cpu.A;
                case Register.B:
                    return cpu.B;
                case Register.C:
                    return cpu.C;
                case Register.D:
                    return cpu.D;
                case Register.E:
                    return cpu.E;
                case Register.H:
                    return cpu.H;
                case Register.I:
                    return cpu.I;
                case Register.L:
                    return cpu.L;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static ushort GetValue(this WideRegister register, Z80Cpu cpu)
        {
            switch (register)
            {
                case WideRegister.AF:
                    return (ushort)(cpu.A << 8 | (byte)cpu.Flags);
                case WideRegister.BC:
                    return (ushort)(cpu.B << 8 | cpu.C);
                case WideRegister.DE:
                    return (ushort)(cpu.D << 8 | cpu.E);
                case WideRegister.HL:
                    return (ushort)(cpu.H << 8 | cpu.L);
                case WideRegister.AF_:
                    return cpu.AF_;
                case WideRegister.BC_:
                    return cpu.BC_;
                case WideRegister.DE_:
                    return cpu.DE_;
                case WideRegister.HL_:
                    return cpu.HL_;
                case WideRegister.SP:
                    return cpu.SP;
                case WideRegister.IX:
                    return cpu.IX;
                case WideRegister.IY:
                    return cpu.IY;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }

        public static void SetValueOnProcessor(this WideRegister register, Z80Cpu cpu, ushort value)
        {
            switch (register)
            {
                case WideRegister.AF:
                    cpu.A = (byte)((value & 0xff00) >> 8);
                    cpu.Flags = (Z80Flags)(value & 0xff);
                    break;
                case WideRegister.BC:
                    cpu.B = (byte)((value & 0xff00) >> 8);
                    cpu.C = (byte)(value & 0xff);
                    break;
                case WideRegister.DE:
                    cpu.D = (byte)((value & 0xff00) >> 8);
                    cpu.E = (byte)(value & 0xff);
                    break;
                case WideRegister.HL:
                    cpu.H = (byte)((value & 0xff00) >> 8);
                    cpu.L = (byte)(value & 0xff);
                    break;
                case WideRegister.SP:
                    cpu.SP = value;
                    break;
                case WideRegister.IX:
                    cpu.IX = value;
                    break;
                case WideRegister.IY:
                    cpu.IY = value;
                    break;
                case WideRegister.AF_:
                    cpu.AF_ = value;
                    break;
                case WideRegister.BC_:
                    cpu.BC_ = value;
                    break;
                case WideRegister.DE_:
                    cpu.DE_ = value;
                    break;
                case WideRegister.HL_:
                    cpu.HL_ = value;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid register value: {register}");
            }
        }
    }

    public class Z80Cpu
    {
        internal IInstruction[] instructions = new IInstruction[65536];
        internal ushort Opcode;

        #region General Purpose Registers
        public byte A;
        public Z80Flags Flags;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public ushort AF_;
        public ushort BC_;
        public ushort DE_;
        public ushort HL_;

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
            if (!_fetchCycle.IsComplete)
            {
                _fetchCycle.Clock();
            }
            if (_fetchCycle.IsComplete && _currentInstruction == null)
            {
                Console.WriteLine("Fetch cycle complete");

                var instruction = instructions[Opcode];

                if (instruction != null)
                {
                    _currentInstruction = instruction;
                    _currentInstruction.Reset();
                    _currentInstruction.StartExecution();
                    if (_currentInstruction.IsComplete)
                    {
                        Opcode = 0x0;
                        _currentInstruction = null;
                        _fetchCycle.Reset();
                    }
                }
                else if (Opcode <= 0xFF)
                {
                    Opcode <<= 8;
                    _fetchCycle.Reset();
                }
                else
                {
                    Console.WriteLine($"Unknown instruction {Opcode:X4}");
                    Opcode = 0x0;
                    _fetchCycle.Reset();
                }
            }
            else if (_currentInstruction != null)
            {
                _currentInstruction.Clock();

                if (_currentInstruction.IsComplete)
                {
                    Opcode = 0x0;
                    _currentInstruction = null;
                    _fetchCycle.Reset();
                }
            }
            TotalTCycles++;
        }

        public void Reset()
        {
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
            MREQ = false;
            WR = false;
            RD = false;
            Opcode = 0;
            _currentInstruction = null;
            _fetchCycle.Reset();
        }

        public void Initialize()
        {
            _fetchCycle = new M1Cycle(this);

            instructions[0x0] = new NOP();

            #region 8 bit LD instructions

            // Load instructions
            instructions[0x02] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.BC), new RegAddrMode8Bit(this, Register.A)); // LD (BC), A
            instructions[0x06] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new ImmediateOperand(this)); // LD B, n
            instructions[0x0a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirectRead(this, WideRegister.BC)); // LD A, (BC))
            instructions[0x0e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new ImmediateOperand(this)); // LD C, n
            instructions[0x12] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.DE), new RegAddrMode8Bit(this, Register.A)); // LD (DE), A
            instructions[0x16] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new ImmediateOperand(this)); // LD D, n
            instructions[0x1a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirectRead(this, WideRegister.DE)); // LD A, (DE))
            instructions[0x1e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new ImmediateOperand(this)); // LD E, n
            instructions[0x26] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new ImmediateOperand(this)); // LD H, n
            instructions[0x2e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new ImmediateOperand(this)); // LD L, n

            instructions[0x32] = new LD_8Bit(this, new ExtendedPointerWrite(this), new RegAddrMode8Bit(this, Register.A)); // LD (nn), A

            instructions[0x36] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new ImmediateOperand(this)); // LD (HL), n

            instructions[0x3a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ExtendedPointerRead8Bit(this)); // LD A,(nn)
            instructions[0x3e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ImmediateOperand(this)); // LD A, n

            instructions[0x40] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.B)); // LD B, B
            instructions[0x41] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.C)); // LD B, C 
            instructions[0x42] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.D)); // LD B, D 
            instructions[0x43] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.E)); // LD B, E
            instructions[0x44] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.H)); // LD B, H
            instructions[0x45] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.L)); // LD B, L
            instructions[0x46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegIndirectRead(this, WideRegister.HL)); // LD B, (HL)
            instructions[0x47] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.A)); // LD B, A

            instructions[0x48] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.B)); // LD C, B
            instructions[0x49] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.C)); // LD C, C 
            instructions[0x4a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.D)); // LD C, D 
            instructions[0x4b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.E)); // LD C, E
            instructions[0x4c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.H)); // LD C, H
            instructions[0x4d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.L)); // LD C, L
            instructions[0x4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegIndirectRead(this, WideRegister.HL)); // LD C, (HL)
            instructions[0x4f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.A)); // LD C, A

            instructions[0x50] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.B)); // LD D, B
            instructions[0x51] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.C)); // LD D, C 
            instructions[0x52] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.D)); // LD D, D 
            instructions[0x53] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.E)); // LD D, E
            instructions[0x54] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.H)); // LD D, H
            instructions[0x55] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.L)); // LD D, L
            instructions[0x56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegIndirectRead(this, WideRegister.HL)); // LD D, (HL)
            instructions[0x57] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.A)); // LD D, A

            instructions[0x58] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.B)); // LD E, B
            instructions[0x59] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.C)); // LD E, C 
            instructions[0x5a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.D)); // LD E, D 
            instructions[0x5b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.E)); // LD E, E
            instructions[0x5c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.H)); // LD E, H
            instructions[0x5d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.L)); // LD E, L
            instructions[0x5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegIndirectRead(this, WideRegister.HL)); // LD E, (HL)
            instructions[0x5f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.A)); // LD E, A

            instructions[0x60] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.B)); // LD H, B
            instructions[0x61] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.C)); // LD H, C 
            instructions[0x62] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.D)); // LD H, D 
            instructions[0x63] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.E)); // LD H, E
            instructions[0x64] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.H)); // LD H, H
            instructions[0x65] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.L)); // LD H, L
            instructions[0x66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegIndirectRead(this, WideRegister.HL)); // LD H, (HL)
            instructions[0x67] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.A)); // LD H, A

            instructions[0x68] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.B)); // LD L, B
            instructions[0x69] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.C)); // LD L, C 
            instructions[0x6a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.D)); // LD L, D 
            instructions[0x6b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.E)); // LD L, E
            instructions[0x6c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.H)); // LD L, H
            instructions[0x6d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.L)); // LD L, L
            instructions[0x6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegIndirectRead(this, WideRegister.HL)); // LD L, (HL)
            instructions[0x6f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.A)); // LD L, A

            instructions[0x70] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.B)); // LD (HL), B
            instructions[0x71] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.C)); // LD (HL), C
            instructions[0x72] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.D)); // LD (HL), D
            instructions[0x73] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.E)); // LD (HL), E
            instructions[0x74] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.H)); // LD (HL), H
            instructions[0x75] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.L)); // LD (HL), L
            instructions[0x77] = new LD_8Bit(this, new RegIndirectWrite(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.A)); // LD (HL), A

            instructions[0x78] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.B)); // LD A, B
            instructions[0x79] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.C)); // LD A, C 
            instructions[0x7a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.D)); // LD A, D 
            instructions[0x7b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.E)); // LD A, E
            instructions[0x7c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.H)); // LD A, H
            instructions[0x7d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.L)); // LD A, L
            instructions[0x7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirectRead(this, WideRegister.HL)); // LD A, (HL)
            instructions[0x7f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.A)); // LD A, A

            instructions[0xdd36] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX, internalCycleLength: 2), new ImmediateOperand(this)); // LD (IX+d), n

            instructions[0xdd46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new IndexedRead(this, WideRegister.IX)); // LD B, (IX+d)
            instructions[0xdd4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new IndexedRead(this, WideRegister.IX)); // LD C, (IX+d)
            instructions[0xdd56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new IndexedRead(this, WideRegister.IX)); // LD D, (IX+d)
            instructions[0xdd5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new IndexedRead(this, WideRegister.IX)); // LD E, (IX+d)
            instructions[0xdd66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new IndexedRead(this, WideRegister.IX)); // LD H, (IX+d)
            instructions[0xdd6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new IndexedRead(this, WideRegister.IX)); // LD L, (IX+d)

            instructions[0xdd70] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.B)); // LD (IX+d), B
            instructions[0xdd71] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.C)); // LD (IX+d), C
            instructions[0xdd72] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.D)); // LD (IX+d), D
            instructions[0xdd73] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.E)); // LD (IX+d), E
            instructions[0xdd74] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.H)); // LD (IX+d), H
            instructions[0xdd75] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.L)); // LD (IX+d), L
            instructions[0xdd77] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.A)); // LD (IX+d), A

            instructions[0xdd7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new IndexedRead(this, WideRegister.IX)); // LD A, (IX+d)

            instructions[0xed47] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.I), new RegAddrMode8Bit(this, Register.A), 1); // LD I, A
            instructions[0xed4f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.R), new RegAddrMode8Bit(this, Register.A), 1); // LD R, A
            instructions[0xed57] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.I), 1); // LD A, I
            instructions[0xed5f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.R), 1); // LD A, R

            instructions[0xfd36] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY, internalCycleLength: 2), new ImmediateOperand(this)); // LD (IY+d), n

            instructions[0xfd46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new IndexedRead(this, WideRegister.IY)); // LD B, (IY+d)
            instructions[0xfd4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new IndexedRead(this, WideRegister.IY)); // LD C, (IY+d)
            instructions[0xfd56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new IndexedRead(this, WideRegister.IY)); // LD D, (IY+d)
            instructions[0xfd5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new IndexedRead(this, WideRegister.IY)); // LD E, (IY+d)
            instructions[0xfd66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new IndexedRead(this, WideRegister.IY)); // LD H, (IY+d)
            instructions[0xfd6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new IndexedRead(this, WideRegister.IY)); // LD L, (IY+d)

            instructions[0xfd70] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.B)); // LD (IY+d), B
            instructions[0xfd71] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.C)); // LD (IY+d), C
            instructions[0xfd72] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.D)); // LD (IY+d), D
            instructions[0xfd73] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.E)); // LD (IY+d), E
            instructions[0xfd74] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.H)); // LD (IY+d), H
            instructions[0xfd75] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.L)); // LD (IY+d), L
            instructions[0xfd77] = new LD_8Bit(this, new IndexedWrite(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.A)); // LD (IY+d), A

            instructions[0xfd7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new IndexedRead(this, WideRegister.IY)); // LD A, (IY+d)

            #endregion

            #region 16 bit LD instructions

            instructions[0x01] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedReadOperand(this)); // LD BC, nn
            instructions[0x11] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedReadOperand(this)); // LD DE, nn
            instructions[0x21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedReadOperand(this)); // LD HL, nn
            instructions[0x22] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.HL)); // LD (nn), HL
            instructions[0x2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedPointerRead16Bit(this)); // LD HL, (nn)
            instructions[0x31] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedReadOperand(this)); // LD SP, nn

            instructions[0xf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.HL), additionalM1TCycles: 2); // LD SP, HL

            instructions[0xdd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedReadOperand(this)); // LD IX, nn
            instructions[0xdd22] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.IX)); // LD (nn), IX
            instructions[0xdd2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedPointerRead16Bit(this)); // LD IX, (nn)
            instructions[0xddf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.IX), additionalM1TCycles: 2); // LD SP, IX

            instructions[0xed43] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.BC)); // LD (nn), BC
            instructions[0xed4b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedPointerRead16Bit(this)); // LD BC, (nn)
            instructions[0xed53] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.DE)); // LD (nn), DE
            instructions[0xed5b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedPointerRead16Bit(this)); // LD DE, (nn)
            instructions[0xed73] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.SP)); // LD (nn), SP
            instructions[0xed7b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedPointerRead16Bit(this)); // LD SP, (nn)

            instructions[0xfd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new ExtendedReadOperand(this)); // LD IY, nn
            instructions[0xfd22] = new LD_16Bit(this, new ExtendedPointerWrite16Bit(this), new RegAddrMode16Bit(this, WideRegister.IY)); // LD (nn), IY
            instructions[0xfd2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new ExtendedPointerRead16Bit(this)); // LD IY, (nn)
            instructions[0xfdf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.IY), additionalM1TCycles: 2); // LD SP, IY

            #endregion

            #region PUSH instructions

            instructions[0xc5] = new PUSH(this, WideRegister.BC); // PUSH BC
            instructions[0xd5] = new PUSH(this, WideRegister.DE); // PUSH DE
            instructions[0xe5] = new PUSH(this, WideRegister.HL); // PUSH HL
            instructions[0xf5] = new PUSH(this, WideRegister.AF); // PUSH AF

            instructions[0xdde5] = new PUSH(this, WideRegister.IX); // PUSH IX
            instructions[0xfde5] = new PUSH(this, WideRegister.IY); // PUSH IX

            #endregion

            #region POP instructions

            instructions[0xc1] = new POP(this, WideRegister.BC); // POP BC
            instructions[0xd1] = new POP(this, WideRegister.DE); // POP DE
            instructions[0xe1] = new POP(this, WideRegister.HL); // POP HL
            instructions[0xf1] = new POP(this, WideRegister.AF); // POP AF

            instructions[0xdde1] = new POP(this, WideRegister.IX); // POP IX
            instructions[0xfde1] = new POP(this, WideRegister.IY); // POP IY

            #endregion

            #region Exchange instructions

            instructions[0x08] = new Exchange(this, WideRegister.AF); // EX AF, AF'
            instructions[0xd9] = new Exchange(this, new[] { WideRegister.BC, WideRegister.DE, WideRegister.HL }); // EXX
            instructions[0xeb] = new Exchange(this, WideRegister.DE, WideRegister.HL); // EX DE, HL

            instructions[0xe3] = new ExchangeStack(this, WideRegister.HL); // EX (SP), HL
            instructions[0xdde3] = new ExchangeStack(this, WideRegister.IX); // EX (SP), IX
            instructions[0xfde3] = new ExchangeStack(this, WideRegister.IY); // EX (SP), IY

            #endregion

            #region Block transfer instructions

            instructions[0xeda0] = new LoadAndXcrement(this, true); // LDI
            instructions[0xedb0] = new LoadAndXcrement(this, true, withRepeat: true); // LDIR
            instructions[0xeda8] = new LoadAndXcrement(this, false); // LDD
            instructions[0xedb8] = new LoadAndXcrement(this, false, withRepeat: true); // LDDR

            #endregion


        }
    }
}
