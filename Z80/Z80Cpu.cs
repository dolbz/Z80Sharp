using System.Threading;
using System.Diagnostics;
using System;
using Z80.Instructions;
using Z80.AddressingModes;

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
                case Register.R:
                    cpu.R = value;
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
                case Register.R:
                    return cpu.R;
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
            instructions[0x02] = new LD_8Bit(this, new RegIndirect(this, WideRegister.BC), new RegAddrMode8Bit(this, Register.A)); // LD (BC), A
            instructions[0x06] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new ImmediateOperand(this)); // LD B, n
            instructions[0x0a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirect(this, WideRegister.BC)); // LD A, (BC))
            instructions[0x0e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new ImmediateOperand(this)); // LD C, n
            instructions[0x12] = new LD_8Bit(this, new RegIndirect(this, WideRegister.DE), new RegAddrMode8Bit(this, Register.A)); // LD (DE), A
            instructions[0x16] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new ImmediateOperand(this)); // LD D, n
            instructions[0x1a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirect(this, WideRegister.DE)); // LD A, (DE))
            instructions[0x1e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new ImmediateOperand(this)); // LD E, n
            instructions[0x26] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new ImmediateOperand(this)); // LD H, n
            instructions[0x2e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new ImmediateOperand(this)); // LD L, n

            instructions[0x32] = new LD_8Bit(this, new ExtendedPointer(this), new RegAddrMode8Bit(this, Register.A)); // LD (nn), A

            instructions[0x36] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new ImmediateOperand(this)); // LD (HL), n

            instructions[0x3a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ExtendedPointer(this)); // LD A,(nn)
            instructions[0x3e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new ImmediateOperand(this)); // LD A, n

            instructions[0x40] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.B)); // LD B, B
            instructions[0x41] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.C)); // LD B, C 
            instructions[0x42] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.D)); // LD B, D 
            instructions[0x43] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.E)); // LD B, E
            instructions[0x44] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.H)); // LD B, H
            instructions[0x45] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.L)); // LD B, L
            instructions[0x46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegIndirect(this, WideRegister.HL)); // LD B, (HL)
            instructions[0x47] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new RegAddrMode8Bit(this, Register.A)); // LD B, A

            instructions[0x48] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.B)); // LD C, B
            instructions[0x49] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.C)); // LD C, C 
            instructions[0x4a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.D)); // LD C, D 
            instructions[0x4b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.E)); // LD C, E
            instructions[0x4c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.H)); // LD C, H
            instructions[0x4d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.L)); // LD C, L
            instructions[0x4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegIndirect(this, WideRegister.HL)); // LD C, (HL)
            instructions[0x4f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new RegAddrMode8Bit(this, Register.A)); // LD C, A

            instructions[0x50] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.B)); // LD D, B
            instructions[0x51] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.C)); // LD D, C 
            instructions[0x52] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.D)); // LD D, D 
            instructions[0x53] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.E)); // LD D, E
            instructions[0x54] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.H)); // LD D, H
            instructions[0x55] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.L)); // LD D, L
            instructions[0x56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegIndirect(this, WideRegister.HL)); // LD D, (HL)
            instructions[0x57] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new RegAddrMode8Bit(this, Register.A)); // LD D, A

            instructions[0x58] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.B)); // LD E, B
            instructions[0x59] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.C)); // LD E, C 
            instructions[0x5a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.D)); // LD E, D 
            instructions[0x5b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.E)); // LD E, E
            instructions[0x5c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.H)); // LD E, H
            instructions[0x5d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.L)); // LD E, L
            instructions[0x5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegIndirect(this, WideRegister.HL)); // LD E, (HL)
            instructions[0x5f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new RegAddrMode8Bit(this, Register.A)); // LD E, A

            instructions[0x60] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.B)); // LD H, B
            instructions[0x61] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.C)); // LD H, C 
            instructions[0x62] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.D)); // LD H, D 
            instructions[0x63] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.E)); // LD H, E
            instructions[0x64] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.H)); // LD H, H
            instructions[0x65] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.L)); // LD H, L
            instructions[0x66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegIndirect(this, WideRegister.HL)); // LD H, (HL)
            instructions[0x67] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new RegAddrMode8Bit(this, Register.A)); // LD H, A

            instructions[0x68] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.B)); // LD L, B
            instructions[0x69] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.C)); // LD L, C 
            instructions[0x6a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.D)); // LD L, D 
            instructions[0x6b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.E)); // LD L, E
            instructions[0x6c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.H)); // LD L, H
            instructions[0x6d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.L)); // LD L, L
            instructions[0x6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegIndirect(this, WideRegister.HL)); // LD L, (HL)
            instructions[0x6f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new RegAddrMode8Bit(this, Register.A)); // LD L, A

            instructions[0x70] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.B)); // LD (HL), B
            instructions[0x71] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.C)); // LD (HL), C
            instructions[0x72] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.D)); // LD (HL), D
            instructions[0x73] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.E)); // LD (HL), E
            instructions[0x74] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.H)); // LD (HL), H
            instructions[0x75] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.L)); // LD (HL), L
            instructions[0x77] = new LD_8Bit(this, new RegIndirect(this, WideRegister.HL), new RegAddrMode8Bit(this, Register.A)); // LD (HL), A

            instructions[0x78] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.B)); // LD A, B
            instructions[0x79] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.C)); // LD A, C 
            instructions[0x7a] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.D)); // LD A, D 
            instructions[0x7b] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.E)); // LD A, E
            instructions[0x7c] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.H)); // LD A, H
            instructions[0x7d] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.L)); // LD A, L
            instructions[0x7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegIndirect(this, WideRegister.HL)); // LD A, (HL)
            instructions[0x7f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.A)); // LD A, A

            instructions[0xdd36] = new LD_8Bit(this, new Indexed(this, WideRegister.IX, internalCycleLength: 2), new ImmediateOperand(this)); // LD (IX+d), n

            instructions[0xdd46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new Indexed(this, WideRegister.IX)); // LD B, (IX+d)
            instructions[0xdd4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new Indexed(this, WideRegister.IX)); // LD C, (IX+d)
            instructions[0xdd56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new Indexed(this, WideRegister.IX)); // LD D, (IX+d)
            instructions[0xdd5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new Indexed(this, WideRegister.IX)); // LD E, (IX+d)
            instructions[0xdd66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new Indexed(this, WideRegister.IX)); // LD H, (IX+d)
            instructions[0xdd6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new Indexed(this, WideRegister.IX)); // LD L, (IX+d)

            instructions[0xdd70] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.B)); // LD (IX+d), B
            instructions[0xdd71] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.C)); // LD (IX+d), C
            instructions[0xdd72] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.D)); // LD (IX+d), D
            instructions[0xdd73] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.E)); // LD (IX+d), E
            instructions[0xdd74] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.H)); // LD (IX+d), H
            instructions[0xdd75] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.L)); // LD (IX+d), L
            instructions[0xdd77] = new LD_8Bit(this, new Indexed(this, WideRegister.IX), new RegAddrMode8Bit(this, Register.A)); // LD (IX+d), A

            instructions[0xdd7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new Indexed(this, WideRegister.IX)); // LD A, (IX+d)

            instructions[0xed47] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.I), new RegAddrMode8Bit(this, Register.A), 1); // LD I, A
            instructions[0xed4f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.R), new RegAddrMode8Bit(this, Register.A), 1); // LD R, A
            instructions[0xed57] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.I), 1); // LD A, I
            instructions[0xed5f] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new RegAddrMode8Bit(this, Register.R), 1); // LD A, R

            instructions[0xfd36] = new LD_8Bit(this, new Indexed(this, WideRegister.IY, internalCycleLength: 2), new ImmediateOperand(this)); // LD (IY+d), n

            instructions[0xfd46] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.B), new Indexed(this, WideRegister.IY)); // LD B, (IY+d)
            instructions[0xfd4e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.C), new Indexed(this, WideRegister.IY)); // LD C, (IY+d)
            instructions[0xfd56] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.D), new Indexed(this, WideRegister.IY)); // LD D, (IY+d)
            instructions[0xfd5e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.E), new Indexed(this, WideRegister.IY)); // LD E, (IY+d)
            instructions[0xfd66] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.H), new Indexed(this, WideRegister.IY)); // LD H, (IY+d)
            instructions[0xfd6e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.L), new Indexed(this, WideRegister.IY)); // LD L, (IY+d)

            instructions[0xfd70] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.B)); // LD (IY+d), B
            instructions[0xfd71] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.C)); // LD (IY+d), C
            instructions[0xfd72] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.D)); // LD (IY+d), D
            instructions[0xfd73] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.E)); // LD (IY+d), E
            instructions[0xfd74] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.H)); // LD (IY+d), H
            instructions[0xfd75] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.L)); // LD (IY+d), L
            instructions[0xfd77] = new LD_8Bit(this, new Indexed(this, WideRegister.IY), new RegAddrMode8Bit(this, Register.A)); // LD (IY+d), A

            instructions[0xfd7e] = new LD_8Bit(this, new RegAddrMode8Bit(this, Register.A), new Indexed(this, WideRegister.IY)); // LD A, (IY+d)

            #endregion

            #region 16 bit LD instructions

            instructions[0x01] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedReadOperand(this)); // LD BC, nn
            instructions[0x11] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedReadOperand(this)); // LD DE, nn
            instructions[0x21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedReadOperand(this)); // LD HL, nn
            instructions[0x22] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.HL)); // LD (nn), HL
            instructions[0x2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedPointer16Bit(this)); // LD HL, (nn)
            instructions[0x31] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedReadOperand(this)); // LD SP, nn

            instructions[0xf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.HL), additionalM1TCycles: 2); // LD SP, HL

            instructions[0xdd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedReadOperand(this)); // LD IX, nn
            instructions[0xdd22] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.IX)); // LD (nn), IX
            instructions[0xdd2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedPointer16Bit(this)); // LD IX, (nn)
            instructions[0xddf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.IX), additionalM1TCycles: 2); // LD SP, IX

            instructions[0xed43] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.BC)); // LD (nn), BC
            instructions[0xed4b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedPointer16Bit(this)); // LD BC, (nn)
            instructions[0xed53] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.DE)); // LD (nn), DE
            instructions[0xed5b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedPointer16Bit(this)); // LD DE, (nn)
            instructions[0xed73] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.SP)); // LD (nn), SP
            instructions[0xed7b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedPointer16Bit(this)); // LD SP, (nn)

            instructions[0xfd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new ExtendedReadOperand(this)); // LD IY, nn
            instructions[0xfd22] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.IY)); // LD (nn), IY
            instructions[0xfd2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new ExtendedPointer16Bit(this)); // LD IY, (nn)
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

            #region Block search instructions
            
            instructions[0xeda1] = new CompareAndXcrement(this, true); // CPI
            instructions[0xedb1] = new CompareAndXcrement(this, true, withRepeat: true); // CPIR
            instructions[0xeda9] = new CompareAndXcrement(this, false); // CPD
            instructions[0xedb9] = new CompareAndXcrement(this, false, withRepeat: true); // CPDR

            #endregion

            #region 8-bit arithmetic and logic

            instructions[0x87] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.A)); // ADD A, A
            instructions[0x80] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.B)); // ADD A, B
            instructions[0x81] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.C)); // ADD A, C
            instructions[0x82] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.D)); // ADD A, D
            instructions[0x83] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.E)); // ADD A, E
            instructions[0x84] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.H)); // ADD A, H
            instructions[0x85] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.L)); // ADD A, L
            instructions[0x86] = new Add_8bit(this, new RegIndirect(this, WideRegister.HL)); // ADD A, (HL)
            instructions[0xdd86] = new Add_8bit(this, new Indexed(this, WideRegister.IX)); // ADD A, (IX+d)
            instructions[0xfd86] = new Add_8bit(this, new Indexed(this, WideRegister.IY)); // ADD A, (IY+d)
            instructions[0xc6] = new Add_8bit(this, new ImmediateOperand(this)); // ADD A, n

            instructions[0x8f] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.A), true); // ADC A, B
            instructions[0x88] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.B), true); // ADC A, B
            instructions[0x89] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.C), true); // ADC A, C
            instructions[0x8a] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.D), true); // ADC A, D
            instructions[0x8b] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.E), true); // ADC A, E
            instructions[0x8c] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.H), true); // ADC A, H
            instructions[0x8d] = new Add_8bit(this, new RegAddrMode8Bit(this, Register.L), true); // ADC A, L
            instructions[0x8e] = new Add_8bit(this, new RegIndirect(this, WideRegister.HL), true); // ADC A, (HL)
            instructions[0xdd8e] = new Add_8bit(this, new Indexed(this, WideRegister.IX), true); // ADC A, (IX+d)
            instructions[0xfd8e] = new Add_8bit(this, new Indexed(this, WideRegister.IY), true); // ADC A, (IY+d)
            instructions[0xce] = new Add_8bit(this, new ImmediateOperand(this), true); // ADC A, n

            instructions[0x97] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.A)); // SUB A, A
            instructions[0x90] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.B)); // SUB A, B
            instructions[0x91] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.C)); // SUB A, C
            instructions[0x92] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.D)); // SUB A, D
            instructions[0x93] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.E)); // SUB A, E
            instructions[0x94] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.H)); // SUB A, H
            instructions[0x95] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.L)); // SUB A, L
            instructions[0x96] = new SubtractOrCompare(this, new RegIndirect(this, WideRegister.HL)); // SUB A, (HL)
            instructions[0xdd96] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IX)); // SUB A, (IX+d)
            instructions[0xfd96] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IY)); // SUB A, (IY+d)
            instructions[0xd6] = new SubtractOrCompare(this, new ImmediateOperand(this)); // SUB A, n

            instructions[0x9f] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.A), true); // SBC A, B
            instructions[0x98] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.B), true); // SBC A, B
            instructions[0x99] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.C), true); // SBC A, C
            instructions[0x9a] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.D), true); // SBC A, D
            instructions[0x9b] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.E), true); // SBC A, E
            instructions[0x9c] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.H), true); // SBC A, H
            instructions[0x9d] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.L), true); // SBC A, L
            instructions[0x9e] = new SubtractOrCompare(this, new RegIndirect(this, WideRegister.HL), true); // SBC A, (HL)
            instructions[0xdd9e] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IX), true); // SBC A, (IX+d)
            instructions[0xfd9e] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IY), true); // SBC A, (IY+d)
            instructions[0xde] = new SubtractOrCompare(this, new ImmediateOperand(this), true); // SBC A, n

            instructions[0xa7] = new AND(this, new RegAddrMode8Bit(this, Register.A)); // AND A, B
            instructions[0xa0] = new AND(this, new RegAddrMode8Bit(this, Register.B)); // AND A, B
            instructions[0xa1] = new AND(this, new RegAddrMode8Bit(this, Register.C)); // AND A, C
            instructions[0xa2] = new AND(this, new RegAddrMode8Bit(this, Register.D)); // AND A, D
            instructions[0xa3] = new AND(this, new RegAddrMode8Bit(this, Register.E)); // AND A, E
            instructions[0xa4] = new AND(this, new RegAddrMode8Bit(this, Register.H)); // AND A, H
            instructions[0xa5] = new AND(this, new RegAddrMode8Bit(this, Register.L)); // AND A, L
            instructions[0xa6] = new AND(this, new RegIndirect(this, WideRegister.HL)); // AND A, (HL)
            instructions[0xdda6] = new AND(this, new Indexed(this, WideRegister.IX)); // AND A, (IX+d)
            instructions[0xfda6] = new AND(this, new Indexed(this, WideRegister.IY)); // AND A, (IY+d)
            instructions[0xe6] = new AND(this, new ImmediateOperand(this)); // AND A, n

            instructions[0xaf] = new XOR(this, new RegAddrMode8Bit(this, Register.A)); // XOR A, B
            instructions[0xa8] = new XOR(this, new RegAddrMode8Bit(this, Register.B)); // XOR A, B
            instructions[0xa9] = new XOR(this, new RegAddrMode8Bit(this, Register.C)); // XOR A, C
            instructions[0xaa] = new XOR(this, new RegAddrMode8Bit(this, Register.D)); // XOR A, D
            instructions[0xab] = new XOR(this, new RegAddrMode8Bit(this, Register.E)); // XOR A, E
            instructions[0xac] = new XOR(this, new RegAddrMode8Bit(this, Register.H)); // XOR A, H
            instructions[0xad] = new XOR(this, new RegAddrMode8Bit(this, Register.L)); // XOR A, L
            instructions[0xae] = new XOR(this, new RegIndirect(this, WideRegister.HL)); // XOR A, (HL)
            instructions[0xddae] = new XOR(this, new Indexed(this, WideRegister.IX)); // XOR A, (IX+d)
            instructions[0xfdae] = new XOR(this, new Indexed(this, WideRegister.IY)); // XOR A, (IY+d)
            instructions[0xee] = new XOR(this, new ImmediateOperand(this)); // XOR A, n

            instructions[0xb7] = new OR(this, new RegAddrMode8Bit(this, Register.A)); // OR A, B
            instructions[0xb0] = new OR(this, new RegAddrMode8Bit(this, Register.B)); // OR A, B
            instructions[0xb1] = new OR(this, new RegAddrMode8Bit(this, Register.C)); // OR A, C
            instructions[0xb2] = new OR(this, new RegAddrMode8Bit(this, Register.D)); // OR A, D
            instructions[0xb3] = new OR(this, new RegAddrMode8Bit(this, Register.E)); // OR A, E
            instructions[0xb4] = new OR(this, new RegAddrMode8Bit(this, Register.H)); // OR A, H
            instructions[0xb5] = new OR(this, new RegAddrMode8Bit(this, Register.L)); // OR A, L
            instructions[0xb6] = new OR(this, new RegIndirect(this, WideRegister.HL)); // OR A, (HL)
            instructions[0xddb6] = new OR(this, new Indexed(this, WideRegister.IX)); // OR A, (IX+d)
            instructions[0xfdb6] = new OR(this, new Indexed(this, WideRegister.IY)); // OR A, (IY+d)
            instructions[0xf6] = new OR(this, new ImmediateOperand(this)); // OR A, n

            instructions[0xbf] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.A), updateAccumulator: false); // CP A, B
            instructions[0xb8] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.B), updateAccumulator: false); // CP A, B
            instructions[0xb9] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.C), updateAccumulator: false); // CP A, C
            instructions[0xba] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.D), updateAccumulator: false); // CP A, D
            instructions[0xbb] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.E), updateAccumulator: false); // CP A, E
            instructions[0xbc] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.H), updateAccumulator: false); // CP A, H
            instructions[0xbd] = new SubtractOrCompare(this, new RegAddrMode8Bit(this, Register.L), updateAccumulator: false); // CP A, L
            instructions[0xbe] = new SubtractOrCompare(this, new RegIndirect(this, WideRegister.HL), updateAccumulator: false); // CP A, (HL)
            instructions[0xddbe] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IX), updateAccumulator: false); // CP A, (IX+d)
            instructions[0xfdbe] = new SubtractOrCompare(this, new Indexed(this, WideRegister.IY), updateAccumulator: false); // CP A, (IY+d)
            instructions[0xfe] = new SubtractOrCompare(this, new ImmediateOperand(this), updateAccumulator: false); // CP A, n

            instructions[0x3c] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.A)); // INC A
            instructions[0x04] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.B)); // INC B
            instructions[0x0c] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.C)); // INC C
            instructions[0x14] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.D)); // INC D
            instructions[0x1c] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.E)); // INC E
            instructions[0x24] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.H)); // INC H
            instructions[0x2c] = new Increment_8bit(this, new RegAddrMode8Bit(this, Register.L)); // INC L
            instructions[0x34] = new Increment_8bit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true)); // INC (HL)
            instructions[0xdd34] = new Increment_8bit(this, new Indexed(this, WideRegister.IX, additionalCycleOnRead: true)); // INC (IX+d)
            instructions[0xfd34] = new Increment_8bit(this, new Indexed(this, WideRegister.IY, additionalCycleOnRead: true)); // INC (IY+d)

            instructions[0x3d] = new Decrement(this, new RegAddrMode8Bit(this, Register.A)); // DEC A
            instructions[0x05] = new Decrement(this, new RegAddrMode8Bit(this, Register.B)); // DEC B
            instructions[0x0d] = new Decrement(this, new RegAddrMode8Bit(this, Register.C)); // DEC C
            instructions[0x15] = new Decrement(this, new RegAddrMode8Bit(this, Register.D)); // DEC D
            instructions[0x1d] = new Decrement(this, new RegAddrMode8Bit(this, Register.E)); // DEC E
            instructions[0x25] = new Decrement(this, new RegAddrMode8Bit(this, Register.H)); // DEC H
            instructions[0x2d] = new Decrement(this, new RegAddrMode8Bit(this, Register.L)); // DEC L
            instructions[0x35] = new Decrement(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true)); // DEC (HL)
            instructions[0xdd35] = new Decrement(this, new Indexed(this, WideRegister.IX, additionalCycleOnRead: true)); // DEC (IX+d)
            instructions[0xfd35] = new Decrement(this, new Indexed(this, WideRegister.IY, additionalCycleOnRead: true)); // DEC (IY+d)

            #endregion

            #region General purpose AF operations

            instructions[0x27] = new DecimalAdjustAccumulator(this); // DAA
            instructions[0x2f] = new ComplementAccumulator(this); // CPL
            instructions[0xed44] = new NegateAccumulator(this); // NEG
            instructions[0x3f] = new ComplementCarryFlag(this); // CCF
            instructions[0x37] = new SetCarryFlag(this); // SCF

            #endregion

            #region 16-bit arithmetic

            instructions[0x09] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.BC)); // ADD HL, BC
            instructions[0x19] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.DE)); // ADD HL, DE
            instructions[0x29] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.HL)); // ADD HL, HL
            instructions[0x39] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.SP)); // ADD HL, SP
            
            instructions[0xdd09] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new RegAddrMode16Bit(this, WideRegister.BC)); // ADD IX, BC
            instructions[0xdd19] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new RegAddrMode16Bit(this, WideRegister.DE)); // ADD IX, DE
            instructions[0xdd39] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new RegAddrMode16Bit(this, WideRegister.SP)); // ADD IX, SP
            instructions[0xdd29] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new RegAddrMode16Bit(this, WideRegister.IX)); // ADD IX, IX
            
            instructions[0xfd09] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new RegAddrMode16Bit(this, WideRegister.BC)); // ADD IY, BC
            instructions[0xfd19] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new RegAddrMode16Bit(this, WideRegister.DE)); // ADD IY, DE
            instructions[0xfd39] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new RegAddrMode16Bit(this, WideRegister.SP)); // ADD IY, SP
            instructions[0xfd29] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new RegAddrMode16Bit(this, WideRegister.IY)); // ADD IY, IY

            instructions[0xed4a] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.BC), withCarry: true); // ADC HL, BC
            instructions[0xed5a] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.DE), withCarry: true); // ADC HL, DE
            instructions[0xed6a] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.HL), withCarry: true); // ADC HL, HL
            instructions[0xed7a] = new Add_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.SP), withCarry: true); // ADC HL, SP

            instructions[0xed42] = new SubtractWithCarry_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.BC)); // SBC HL, BC
            instructions[0xed52] = new SubtractWithCarry_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.DE)); // SBC HL, DE
            instructions[0xed62] = new SubtractWithCarry_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.HL)); // SBC HL, HL
            instructions[0xed72] = new SubtractWithCarry_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new RegAddrMode16Bit(this, WideRegister.SP)); // SBC HL, SP
            
            instructions[0x03] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.BC)); // INC BC
            instructions[0x13] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.DE)); // INC DE
            instructions[0x23] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL)); // INC HL
            instructions[0x33] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.SP)); // INC SP
            instructions[0xdd23] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX)); // INC IX
            instructions[0xfd23] = new Increment_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY)); // INC IY

            instructions[0x0b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.BC)); // DEC BC
            instructions[0x1b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.DE)); // DEC DE
            instructions[0x2b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.HL)); // DEC HL
            instructions[0x3b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.SP)); // DEC SP
            instructions[0xdd2b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.IX)); // DEC IX
            instructions[0xfd2b] = new Decrement_16bit(this, new RegAddrMode16Bit(this, WideRegister.IY)); // DEC IY

            #endregion

            #region Rotates and shifts

            // 8080 compatible instructions
            instructions[0x07] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.A), circular: true, is8080Compatible: true); // RLCA
            instructions[0x0f] = new RotateRight(this, new RegAddrMode8Bit(this, Register.A), circular: true, is8080Compatible: true); // RRCA
            instructions[0x17] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.A), is8080Compatible: true); // RLA
            instructions[0x1f] = new RotateRight(this, new RegAddrMode8Bit(this, Register.A), is8080Compatible: true); // RRA

            //  Z80 added instructions
            instructions[0xcb07] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.A), circular: true); // RLC A
            instructions[0xcb00] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.B), circular: true); // RLC B
            instructions[0xcb01] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.C), circular: true); // RLC C
            instructions[0xcb02] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.D), circular: true); // RLC D
            instructions[0xcb03] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.E), circular: true); // RLC E
            instructions[0xcb04] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.H), circular: true); // RLC H
            instructions[0xcb05] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.L), circular: true); // RLC L
            instructions[0xcb06] = new RotateLeft(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), circular: true); // RLC (HL)

            instructions[0xcb0f] = new RotateRight(this, new RegAddrMode8Bit(this, Register.A), circular: true); // RRC A
            instructions[0xcb08] = new RotateRight(this, new RegAddrMode8Bit(this, Register.B), circular: true); // RRC B
            instructions[0xcb09] = new RotateRight(this, new RegAddrMode8Bit(this, Register.C), circular: true); // RRC C
            instructions[0xcb0a] = new RotateRight(this, new RegAddrMode8Bit(this, Register.D), circular: true); // RRC D
            instructions[0xcb0b] = new RotateRight(this, new RegAddrMode8Bit(this, Register.E), circular: true); // RRC E
            instructions[0xcb0c] = new RotateRight(this, new RegAddrMode8Bit(this, Register.H), circular: true); // RRC H
            instructions[0xcb0d] = new RotateRight(this, new RegAddrMode8Bit(this, Register.L), circular: true); // RRC L
            instructions[0xcb0e] = new RotateRight(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), circular: true); // RRC (HL)

            instructions[0xcb17] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.A)); // RL A
            instructions[0xcb10] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.B)); // RL B
            instructions[0xcb11] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.C)); // RL C
            instructions[0xcb12] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.D)); // RL D
            instructions[0xcb13] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.E)); // RL E
            instructions[0xcb14] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.H)); // RL H
            instructions[0xcb15] = new RotateLeft(this, new RegAddrMode8Bit(this, Register.L)); // RL L
            instructions[0xcb16] = new RotateLeft(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true)); // RL (HL)

            instructions[0xcb1f] = new RotateRight(this, new RegAddrMode8Bit(this, Register.A)); // RR A
            instructions[0xcb18] = new RotateRight(this, new RegAddrMode8Bit(this, Register.B)); // RR B
            instructions[0xcb19] = new RotateRight(this, new RegAddrMode8Bit(this, Register.C)); // RR C
            instructions[0xcb1a] = new RotateRight(this, new RegAddrMode8Bit(this, Register.D)); // RR D
            instructions[0xcb1b] = new RotateRight(this, new RegAddrMode8Bit(this, Register.E)); // RR E
            instructions[0xcb1c] = new RotateRight(this, new RegAddrMode8Bit(this, Register.H)); // RR H
            instructions[0xcb1d] = new RotateRight(this, new RegAddrMode8Bit(this, Register.L)); // RR L
            instructions[0xcb1e] = new RotateRight(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead : true)); // RR (HL)

            instructions[0xcb27] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.A)); // SLA A
            instructions[0xcb20] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.B)); // SLA B
            instructions[0xcb21] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.C)); // SLA C
            instructions[0xcb22] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.D)); // SLA D
            instructions[0xcb23] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.E)); // SLA E
            instructions[0xcb24] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.H)); // SLA H
            instructions[0xcb25] = new ShiftLeftArithmetic(this, new RegAddrMode8Bit(this, Register.L)); // SLA L
            instructions[0xcb26] = new ShiftLeftArithmetic(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true)); // SLA (HL)

            instructions[0xcb2f] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.A)); // SRA A
            instructions[0xcb28] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.B)); // SRA B
            instructions[0xcb29] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.C)); // SRA C
            instructions[0xcb2a] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.D)); // SRA D
            instructions[0xcb2b] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.E)); // SRA E
            instructions[0xcb2c] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.H)); // SRA H
            instructions[0xcb2d] = new ShiftRightArithmetic(this, new RegAddrMode8Bit(this, Register.L)); // SRA L
            instructions[0xcb2e] = new ShiftRightArithmetic(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead : true)); // SRA (HL)

            instructions[0xcb3f] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.A)); // SRL A
            instructions[0xcb38] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.B)); // SRL B
            instructions[0xcb39] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.C)); // SRL C
            instructions[0xcb3a] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.D)); // SRL D
            instructions[0xcb3b] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.E)); // SRL E
            instructions[0xcb3c] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.H)); // SRL H
            instructions[0xcb3d] = new ShiftRightLogical(this, new RegAddrMode8Bit(this, Register.L)); // SRL L
            instructions[0xcb3e] = new ShiftRightLogical(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead : true)); // SRL (HL)

            instructions[0xddcb] = new RotateIndexed(this, WideRegister.IX); // Covers all (IX+d) rotates and shifts
            instructions[0xfdcb] = new RotateIndexed(this, WideRegister.IY); // Covers all (IY+d) rotates and shifts

            #endregion
        }
    }
}
