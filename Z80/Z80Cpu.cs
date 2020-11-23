using System;
using Z80.Instructions;
using Z80.AddressingModes;
using Z80.Instructions.InterruptHandlers;

namespace Z80
{
    public class Z80Cpu
    {
        public readonly object CpuStateLock = new object();

        public IInstruction[] instructions = new IInstruction[65536];
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

        public bool M1;

        public bool RFRSH;

        public bool HALT { get; internal set; }
        public bool NMI { 
            set {
                if (value) {
                    PendingNMI = true;
                }
            } 
        }

        public bool INT;
        
        internal bool PendingNMI { get; set; }
        internal bool PendingINT {get;set;}
        internal bool IFF1;
        internal bool IFF2;

        public bool NewInstruction;
        internal int InterruptMode = 0;

        public long TotalTCycles { get; private set; } = 0;

        internal M1Cycle _fetchCycle;
        internal IInstruction _currentInstruction;
        internal bool PendingEI;

        internal ushort PostIncrementPC() {
            if (!HALT || PendingNMI || PendingINT) {
                    return PC++;
            }
            return PC;
        }
        public void Clock()
        {
            NewInstruction = false;
            if (!_fetchCycle.IsComplete)
            {
                _fetchCycle.Clock();
            }
            if (_fetchCycle.IsComplete && _currentInstruction == null)
            {
                // Check for BUSRQ

                IInstruction instruction = null;
                
                PostIncrementPC(); // TODO must stop increment if pending interrupt

                if (!HALT) {
                    instruction = instructions[Opcode];
                }

                if (!HALT || PendingNMI) {
                    if (instruction != null)
                    {
                        HALT = false; // Break out of the halt mode as we must have been interrupted
                        PendingNMI = false;
                        _currentInstruction = instruction;
                        _currentInstruction.Reset();
                        _currentInstruction.StartExecution();
                        SetupForNextInstructionIfRequired();
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
            }
            else if (_currentInstruction != null)
            {
                RFRSH = false; // TODO this isn't correct for instructions that extended the M1 cycle
                _currentInstruction.Clock();
                SetupForNextInstructionIfRequired();
            }
            TotalTCycles++;
        }

        private void SetupForNextInstructionIfRequired() {
            if (_currentInstruction.IsComplete)
            {
                if (IFF1 && INT) {
                    PendingINT = true; // This is at the end the last clock cycle rather than at the start...proably not a big deal
                    IFF1 = false;
                }

                if (PendingEI && !(_currentInstruction is EI)) {
                    IFF1 = true;
                    IFF2 = true;
                    PendingEI = false;
                }
                if (PendingNMI) {
                    _currentInstruction = new NMIHandler(this);
                } else if (PendingINT) {
                    PendingINT = false;
                    switch (InterruptMode) {
                        case 0:
                            _currentInstruction = new Mode0Handler();
                        break;
                        case 1:
                            _currentInstruction = new Mode1Handler(this);
                        break;
                        case 2:
                            _currentInstruction = new Mode2Handler();
                        break;
                        default:
                            throw new InvalidOperationException("Invalid interrupt mode set on CPU");
                    }
                } else {
                    Opcode = 0x0;
                    _currentInstruction = null;
                }

                NewInstruction = true;
                _fetchCycle.Reset();
            }
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

            Initialize();
            MREQ = false;
            WR = false;
            RD = false;
            M1 = false;
            RFRSH = false;
            Opcode = 0;
            _currentInstruction = null;
            _fetchCycle.Reset();
            Flags = 0;
            InterruptMode = 0;
            HALT = false;
            IFF1 = false;
            TotalTCycles = 0;
            NewInstruction = true;
        }


        public Z80CpuSnapshot GetStateSnapshot() {
            return Z80CpuSnapshot.FromCpu(this);
        }

        public void Initialize()
        {
            _fetchCycle = new M1Cycle(this);

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

            instructions[0x01] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedAddressMode(this)); // LD BC, nn
            instructions[0x11] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedAddressMode(this)); // LD DE, nn
            instructions[0x21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedAddressMode(this)); // LD HL, nn
            instructions[0x22] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.HL)); // LD (nn), HL
            instructions[0x2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.HL), new ExtendedPointer16Bit(this)); // LD HL, (nn)
            instructions[0x31] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedAddressMode(this)); // LD SP, nn

            instructions[0xf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.HL), additionalM1TCycles: 2); // LD SP, HL

            instructions[0xdd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedAddressMode(this)); // LD IX, nn
            instructions[0xdd22] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.IX)); // LD (nn), IX
            instructions[0xdd2a] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IX), new ExtendedPointer16Bit(this)); // LD IX, (nn)
            instructions[0xddf9] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new RegAddrMode16Bit(this, WideRegister.IX), additionalM1TCycles: 2); // LD SP, IX

            instructions[0xed43] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.BC)); // LD (nn), BC
            instructions[0xed4b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.BC), new ExtendedPointer16Bit(this)); // LD BC, (nn)
            instructions[0xed53] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.DE)); // LD (nn), DE
            instructions[0xed5b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.DE), new ExtendedPointer16Bit(this)); // LD DE, (nn)
            instructions[0xed73] = new LD_16Bit(this, new ExtendedPointer16Bit(this), new RegAddrMode16Bit(this, WideRegister.SP)); // LD (nn), SP
            instructions[0xed7b] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.SP), new ExtendedPointer16Bit(this)); // LD SP, (nn)

            instructions[0xfd21] = new LD_16Bit(this, new RegAddrMode16Bit(this, WideRegister.IY), new ExtendedAddressMode(this)); // LD IY, nn
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

            instructions[0xed6f] = new RotateDigit(this, isLeftShift: true); // RLD (HL)
            instructions[0xed67] = new RotateDigit(this, isLeftShift: false); // RRD (HL)

            #endregion

            #region Bit manipulation

            instructions[0xcb47] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 0); // BIT 0,A
            instructions[0xcb40] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 0); // BIT 0,B
            instructions[0xcb41] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 0); // BIT 0,C
            instructions[0xcb42] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 0); // BIT 0,D
            instructions[0xcb43] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 0); // BIT 0,E
            instructions[0xcb44] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 0); // BIT 0,H
            instructions[0xcb45] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 0); // BIT 0,L
            instructions[0xcb46] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 0); // BIT 0,(HL)

            instructions[0xcb4f] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 1); // BIT 1,A
            instructions[0xcb48] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 1); // BIT 1,B
            instructions[0xcb49] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 1); // BIT 1,C
            instructions[0xcb4a] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 1); // BIT 1,D
            instructions[0xcb4b] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 1); // BIT 1,E
            instructions[0xcb4c] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 1); // BIT 1,H
            instructions[0xcb4d] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 1); // BIT 1,L
            instructions[0xcb4e] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 1); // BIT 1,(HL)

            instructions[0xcb57] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 2); // BIT 2,A
            instructions[0xcb50] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 2); // BIT 2,B
            instructions[0xcb51] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 2); // BIT 2,C
            instructions[0xcb52] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 2); // BIT 2,D
            instructions[0xcb53] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 2); // BIT 2,E
            instructions[0xcb54] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 2); // BIT 2,H
            instructions[0xcb55] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 2); // BIT 2,L
            instructions[0xcb56] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 2); // BIT 2,(HL)

            instructions[0xcb5f] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 3); // BIT 3,A
            instructions[0xcb58] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 3); // BIT 3,B
            instructions[0xcb59] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 3); // BIT 3,C
            instructions[0xcb5a] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 3); // BIT 3,D
            instructions[0xcb5b] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 3); // BIT 3,E
            instructions[0xcb5c] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 3); // BIT 3,H
            instructions[0xcb5d] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 3); // BIT 3,L
            instructions[0xcb5e] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 3); // BIT 3,(HL)

            instructions[0xcb67] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 4); // BIT 4,A
            instructions[0xcb60] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 4); // BIT 4,B
            instructions[0xcb61] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 4); // BIT 4,C
            instructions[0xcb62] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 4); // BIT 4,D
            instructions[0xcb63] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 4); // BIT 4,E
            instructions[0xcb64] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 4); // BIT 4,H
            instructions[0xcb65] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 4); // BIT 4,L
            instructions[0xcb66] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 4); // BIT 4,(HL)

            instructions[0xcb6f] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 5); // BIT 5,A
            instructions[0xcb68] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 5); // BIT 5,B
            instructions[0xcb69] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 5); // BIT 5,C
            instructions[0xcb6a] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 5); // BIT 5,D
            instructions[0xcb6b] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 5); // BIT 5,E
            instructions[0xcb6c] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 5); // BIT 5,H
            instructions[0xcb6d] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 5); // BIT 5,L
            instructions[0xcb6e] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 5); // BIT 5,(HL)

            instructions[0xcb77] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 6); // BIT 6,A
            instructions[0xcb70] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 6); // BIT 6,B
            instructions[0xcb71] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 6); // BIT 6,C
            instructions[0xcb72] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 6); // BIT 6,D
            instructions[0xcb73] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 6); // BIT 6,E
            instructions[0xcb74] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 6); // BIT 6,H
            instructions[0xcb75] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 6); // BIT 6,L
            instructions[0xcb76] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 6); // BIT 6,(HL)

            instructions[0xcb7f] = new BitTest(this, new RegAddrMode8Bit(this, Register.A), 7); // BIT 7,A
            instructions[0xcb78] = new BitTest(this, new RegAddrMode8Bit(this, Register.B), 7); // BIT 7,B
            instructions[0xcb79] = new BitTest(this, new RegAddrMode8Bit(this, Register.C), 7); // BIT 7,C
            instructions[0xcb7a] = new BitTest(this, new RegAddrMode8Bit(this, Register.D), 7); // BIT 7,D
            instructions[0xcb7b] = new BitTest(this, new RegAddrMode8Bit(this, Register.E), 7); // BIT 7,E
            instructions[0xcb7c] = new BitTest(this, new RegAddrMode8Bit(this, Register.H), 7); // BIT 7,H
            instructions[0xcb7d] = new BitTest(this, new RegAddrMode8Bit(this, Register.L), 7); // BIT 7,L
            instructions[0xcb7e] = new BitTest(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 7); // BIT 7,(HL) 


            instructions[0xcb87] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 0, false); // RES 0,A
            instructions[0xcb80] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 0, false); // RES 0,B
            instructions[0xcb81] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 0, false); // RES 0,C
            instructions[0xcb82] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 0, false); // RES 0,D
            instructions[0xcb83] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 0, false); // RES 0,E
            instructions[0xcb84] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 0, false); // RES 0,H
            instructions[0xcb85] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 0, false); // RES 0,L
            instructions[0xcb86] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 0, false); // RES 0,(HL)

            instructions[0xcb8f] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 1, false); // RES 1,A
            instructions[0xcb88] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 1, false); // RES 1,B
            instructions[0xcb89] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 1, false); // RES 1,C
            instructions[0xcb8a] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 1, false); // RES 1,D
            instructions[0xcb8b] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 1, false); // RES 1,E
            instructions[0xcb8c] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 1, false); // RES 1,H
            instructions[0xcb8d] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 1, false); // RES 1,L
            instructions[0xcb8e] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 1, false); // RES 1,(HL)

            instructions[0xcb97] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 2, false); // RES 2,A
            instructions[0xcb90] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 2, false); // RES 2,B
            instructions[0xcb91] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 2, false); // RES 2,C
            instructions[0xcb92] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 2, false); // RES 2,D
            instructions[0xcb93] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 2, false); // RES 2,E
            instructions[0xcb94] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 2, false); // RES 2,H
            instructions[0xcb95] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 2, false); // RES 2,L
            instructions[0xcb96] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 2, false); // RES 2,(HL)

            instructions[0xcb9f] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 3, false); // RES 3,A
            instructions[0xcb98] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 3, false); // RES 3,B
            instructions[0xcb99] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 3, false); // RES 3,C
            instructions[0xcb9a] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 3, false); // RES 3,D
            instructions[0xcb9b] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 3, false); // RES 3,E
            instructions[0xcb9c] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 3, false); // RES 3,H
            instructions[0xcb9d] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 3, false); // RES 3,L
            instructions[0xcb9e] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 3, false); // RES 3,(HL)

            instructions[0xcba7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 4, false); // RES 4,A
            instructions[0xcba0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 4, false); // RES 4,B
            instructions[0xcba1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 4, false); // RES 4,C
            instructions[0xcba2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 4, false); // RES 4,D
            instructions[0xcba3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 4, false); // RES 4,E
            instructions[0xcba4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 4, false); // RES 4,H
            instructions[0xcba5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 4, false); // RES 4,L
            instructions[0xcba6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 4, false); // RES 4,(HL)

            instructions[0xcbaf] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 5, false); // RES 5,A
            instructions[0xcba8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 5, false); // RES 5,B
            instructions[0xcba9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 5, false); // RES 5,C
            instructions[0xcbaa] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 5, false); // RES 5,D
            instructions[0xcbab] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 5, false); // RES 5,E
            instructions[0xcbac] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 5, false); // RES 5,H
            instructions[0xcbad] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 5, false); // RES 5,L
            instructions[0xcbae] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 5, false); // RES 5,(HL)

            instructions[0xcbb7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 6, false); // RES 6,A
            instructions[0xcbb0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 6, false); // RES 6,B
            instructions[0xcbb1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 6, false); // RES 6,C
            instructions[0xcbb2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 6, false); // RES 6,D
            instructions[0xcbb3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 6, false); // RES 6,E
            instructions[0xcbb4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 6, false); // RES 6,H
            instructions[0xcbb5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 6, false); // RES 6,L
            instructions[0xcbb6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 6, false); // RES 6,(HL)

            instructions[0xcbbf] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 7, false); // RES 7,A
            instructions[0xcbb8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 7, false); // RES 7,B
            instructions[0xcbb9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 7, false); // RES 7,C
            instructions[0xcbba] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 7, false); // RES 7,D
            instructions[0xcbbb] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 7, false); // RES 7,E
            instructions[0xcbbc] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 7, false); // RES 7,H
            instructions[0xcbbd] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 7, false); // RES 7,L
            instructions[0xcbbe] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 7, false); // RES 7,(HL)


            instructions[0xcbc7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 0, true); // SET 0,A
            instructions[0xcbc0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 0, true); // SET 0,B
            instructions[0xcbc1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 0, true); // SET 0,C
            instructions[0xcbc2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 0, true); // SET 0,D
            instructions[0xcbc3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 0, true); // SET 0,E
            instructions[0xcbc4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 0, true); // SET 0,H
            instructions[0xcbc5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 0, true); // SET 0,L
            instructions[0xcbc6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 0, true); // SET 0,(HL)

            instructions[0xcbcf] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 1, true); // SET 1,A
            instructions[0xcbc8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 1, true); // SET 1,B
            instructions[0xcbc9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 1, true); // SET 1,C
            instructions[0xcbca] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 1, true); // SET 1,D
            instructions[0xcbcb] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 1, true); // SET 1,E
            instructions[0xcbcc] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 1, true); // SET 1,H
            instructions[0xcbcd] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 1, true); // SET 1,L
            instructions[0xcbce] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 1, true); // SET 1,(HL)

            instructions[0xcbd7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 2, true); // SET 2,A
            instructions[0xcbd0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 2, true); // SET 2,B
            instructions[0xcbd1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 2, true); // SET 2,C
            instructions[0xcbd2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 2, true); // SET 2,D
            instructions[0xcbd3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 2, true); // SET 2,E
            instructions[0xcbd4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 2, true); // SET 2,H
            instructions[0xcbd5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 2, true); // SET 2,L
            instructions[0xcbd6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 2, true); // SET 2,(HL)

            instructions[0xcbdf] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 3, true); // SET 3,A
            instructions[0xcbd8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 3, true); // SET 3,B
            instructions[0xcbd9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 3, true); // SET 3,C
            instructions[0xcbda] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 3, true); // SET 3,D
            instructions[0xcbdb] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 3, true); // SET 3,E
            instructions[0xcbdc] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 3, true); // SET 3,H
            instructions[0xcbdd] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 3, true); // SET 3,L
            instructions[0xcbde] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 3, true); // SET 3,(HL)

            instructions[0xcbe7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 4, true); // SET 4,A
            instructions[0xcbe0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 4, true); // SET 4,B
            instructions[0xcbe1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 4, true); // SET 4,C
            instructions[0xcbe2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 4, true); // SET 4,D
            instructions[0xcbe3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 4, true); // SET 4,E
            instructions[0xcbe4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 4, true); // SET 4,H
            instructions[0xcbe5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 4, true); // SET 4,L
            instructions[0xcbe6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 4, true); // RES 4,(HL)

            instructions[0xcbef] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 5, true); // SET 5,A
            instructions[0xcbe8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 5, true); // SET 5,B
            instructions[0xcbe9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 5, true); // SET 5,C
            instructions[0xcbea] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 5, true); // SET 5,D
            instructions[0xcbeb] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 5, true); // SET 5,E
            instructions[0xcbec] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 5, true); // SET 5,H
            instructions[0xcbed] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 5, true); // SET 5,L
            instructions[0xcbee] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 5, true); // SET 5,(HL)

            instructions[0xcbf7] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 6, true); // SET 6,A
            instructions[0xcbf0] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 6, true); // SET 6,B
            instructions[0xcbf1] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 6, true); // SET 6,C
            instructions[0xcbf2] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 6, true); // SET 6,D
            instructions[0xcbf3] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 6, true); // SET 6,E
            instructions[0xcbf4] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 6, true); // SET 6,H
            instructions[0xcbf5] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 6, true); // SET 6,L
            instructions[0xcbf6] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 6, true); // SET 6,(HL)

            instructions[0xcbff] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.A), 7, true); // SET 7,A
            instructions[0xcbf8] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.B), 7, true); // SET 7,B
            instructions[0xcbf9] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.C), 7, true); // SET 7,C
            instructions[0xcbfa] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.D), 7, true); // SET 7,D
            instructions[0xcbfb] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.E), 7, true); // SET 7,E
            instructions[0xcbfc] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.H), 7, true); // SET 7,H
            instructions[0xcbfd] = new SetOrResetBit(this, new RegAddrMode8Bit(this, Register.L), 7, true); // SET 7,L
            instructions[0xcbfe] = new SetOrResetBit(this, new RegIndirect(this, WideRegister.HL, additionalCycleOnRead: true), 7, true); // SET 7,(HL)

            instructions[0xddcb] = new IndexedInstructionLookup(this, WideRegister.IX); // Covers all (IX+d) rotates, shifts and bit operations
            instructions[0xfdcb] = new IndexedInstructionLookup(this, WideRegister.IY); // Covers all (IY+d) rotates, shifts and bit operations
            
            #endregion

            #region Jump, Call, Return

            instructions[0xc3] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.Unconditional); // JP nn
            instructions[0xda] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.Carry); // JP C,nn
            instructions[0xd2] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.NonCarry); // JP NC,nn
            instructions[0xca] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.Zero); // JP Z,nn
            instructions[0xc2] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.NonZero); // JP NZ,nn
            instructions[0xea] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.ParityEven); // JP PE,nn
            instructions[0xe2] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.ParityOdd); // JP PO,nn
            instructions[0xfa] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.SignNeg); // JP M,nn
            instructions[0xf2] = new Jump(this, new ExtendedAddressMode(this), JumpCondition.SignPos); // JP P,nn
            
            instructions[0x18] = new Jump(this, new RelativeAddressMode(this), JumpCondition.Unconditional, true); // JR, e
            instructions[0x38] = new Jump(this, new RelativeAddressMode(this), JumpCondition.Carry, true); // JR C,e
            instructions[0x30] = new Jump(this, new RelativeAddressMode(this), JumpCondition.NonCarry, true); // JR NC,e
            instructions[0x28] = new Jump(this, new RelativeAddressMode(this), JumpCondition.Zero, true); // JR Z,e
            instructions[0x20] = new Jump(this, new RelativeAddressMode(this), JumpCondition.NonZero, true); // JR NZ,e

            instructions[0xe9] = new Jump(this, new RegAddrMode16Bit(this, WideRegister.HL), JumpCondition.Unconditional); // JP (HL)
            instructions[0xdde9] = new Jump(this, new RegAddrMode16Bit(this, WideRegister.IX), JumpCondition.Unconditional); // JP (IX)
            instructions[0xfde9] = new Jump(this, new RegAddrMode16Bit(this, WideRegister.IX), JumpCondition.Unconditional); // JP (IY)

            instructions[0x10] = new Jump(this, new RelativeAddressMode(this), JumpCondition.RegBNotZero, true, additionalM1TCycles: 1); // DJNZ, e 

            instructions[0xcd] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.Unconditional); // CALL nn
            instructions[0xdc] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.Carry); // CALL C,nn
            instructions[0xd4] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.NonCarry); // CALL NC,nn
            instructions[0xcc] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.Zero); // CALL Z,nn
            instructions[0xc4] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.NonZero); // CALL NZ,nn
            instructions[0xec] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.ParityEven); // CALL PE,nn
            instructions[0xe4] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.ParityOdd); // CALL PO,nn
            instructions[0xfc] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.SignNeg); // CALL M,nn
            instructions[0xf4] = new CALL(this, new ExtendedAddressMode(this), JumpCondition.SignPos); // CALL P,nn

            instructions[0xc9] = new RET(this, JumpCondition.Unconditional); // RET
            instructions[0xd8] = new RET(this, JumpCondition.Carry); // RET C
            instructions[0xd0] = new RET(this, JumpCondition.NonCarry); // RET NC
            instructions[0xc8] = new RET(this, JumpCondition.Zero); // RET Z
            instructions[0xc0] = new RET(this, JumpCondition.NonZero); // RET NZ
            instructions[0xe8] = new RET(this, JumpCondition.ParityEven); // RET PE
            instructions[0xe0] = new RET(this, JumpCondition.ParityOdd); // RET PO
            instructions[0xf8] = new RET(this, JumpCondition.SignNeg); // RET M
            instructions[0xf0] = new RET(this, JumpCondition.SignPos); // RET P

            instructions[0xed4d] = new RETI(this); // RETI
            instructions[0xed45] = new RETN(this); // RETN

            instructions[0xc7] = new RST(this, 0x0); // RST 0 
            instructions[0xcf] = new RST(this, 0x8); // RST 8
            instructions[0xd7] = new RST(this, 0x10); // RST 16
            instructions[0xdf] = new RST(this, 0x18); // RST 24
            instructions[0xe7] = new RST(this, 0x20); // RST 32
            instructions[0xef] = new RST(this, 0x28); // RST 40
            instructions[0xf7] = new RST(this, 0x30); // RST 48
            instructions[0xff] = new RST(this, 0x38); // RST 56

            #endregion

            #region Input 

            // TODO load input into flags register is semi documented here... should we implement?
            instructions[0xdb] = new IN(this, Register.A, new ImmediateOperand(this).Reader, Register.A); // IN A,(n)
            instructions[0xed40] = new IN(this, Register.B, new RegAddrMode8Bit(this, Register.C), Register.B); // IN B (C)
            instructions[0xed48] = new IN(this, Register.C, new RegAddrMode8Bit(this, Register.C), Register.B); // IN C (C)
            instructions[0xed50] = new IN(this, Register.D, new RegAddrMode8Bit(this, Register.C), Register.B); // IN D (C)
            instructions[0xed58] = new IN(this, Register.E, new RegAddrMode8Bit(this, Register.C), Register.B); // IN E (C)
            instructions[0xed60] = new IN(this, Register.H, new RegAddrMode8Bit(this, Register.C), Register.B); // IN H (C)
            instructions[0xed68] = new IN(this, Register.L, new RegAddrMode8Bit(this, Register.C), Register.B); // IN L (C)
            instructions[0xed78] = new IN(this, Register.A, new RegAddrMode8Bit(this, Register.C), Register.B); // IN A (C)

            instructions[0xeda2] = new INxx(this, true, false); // INI
            instructions[0xedaa] = new INxx(this, false, false); // IND

            instructions[0xedb2] = new INxx(this, true, true); // INIR
            instructions[0xedba] = new INxx(this, false, true); // INDR
            #endregion

            #region Output 

            instructions[0xd3] = new OUT(this, new ImmediateOperand(this).Reader, Register.A, Register.A); // OUT (n),A
            instructions[0xed41] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.B, Register.B); // OUT (C), B
            instructions[0xed49] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.C, Register.B); // OUT (C), C
            instructions[0xed51] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.D, Register.B); // OUT (C), D
            instructions[0xed59] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.E, Register.B); // OUT (C), E
            instructions[0xed61] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.H, Register.B); // OUT (C), H
            instructions[0xed69] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.L, Register.B); // OUT (C), L
            instructions[0xed79] = new OUT(this, new RegAddrMode8Bit(this, Register.C), Register.A, Register.B); // OUT (C), A           

            instructions[0xeda3] = new OUTxx(this, true, false); // OUTI
            instructions[0xedab] = new OUTxx(this, false, false); // OUTD

            instructions[0xedb3] = new OUTxx(this, true, true); // OUTIR
            instructions[0xedbb] = new OUTxx(this, false, true); // OUTDR

            #endregion

            #region Misc CPU control

            instructions[0x0] = new NOP(); // NOP
            instructions[0x76] = new HALT(this); // HALT
            instructions[0xf3] = new DI(this); // DI
            instructions[0xfb] = new EI(this); // EI
            instructions[0xed46] = new IM(this, 0); // IM0
            instructions[0xed56] = new IM(this, 1); // IM1
            instructions[0xed5e] = new IM(this, 2); // IM2

            #endregion
        }
    
        // private IInstruction ResolveInterruptPriority() {
        //     if (PendingBusRq) {

        //     } else if ()
        // }
    }
}
