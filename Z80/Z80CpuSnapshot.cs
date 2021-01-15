using System.Runtime.CompilerServices;
namespace Z80 {
    public struct Z80CpuSnapshot {
        public byte A { get; private set; }
        public Z80Flags Flags { get; private set; }
        public byte B { get; private set; }
        public byte C { get; private set; }
        public byte D { get; private set; }
        public byte E { get; private set; }
        public byte H { get; private set; }
        public byte L { get; private set; }
        public ushort AF_ { get; private set; }
        public ushort BC_ { get; private set; }
        public ushort DE_ { get; private set; }
        public ushort HL_ { get; private set; }

        public byte I { get; private set; }
        public byte R { get; private set; }
        public ushort IX { get; private set; }
        public ushort IY { get; private set; }
        public ushort SP { get; private set; }

        public ushort PC { get; private set; }

        public ushort Address { get; private set; }
        public byte Data { get; private set; }

        public bool MREQ { get; private set; }

        public bool IORQ { get; private set; }

        public bool RD { get; private set; }

        public bool WR { get; private set; }

        public bool M1 { get; private set; }

        public bool RFRSH { get; private set; }

        public bool NewInstruction;

        internal static Z80CpuSnapshot FromCpu(Z80Cpu cpu) {
            lock (cpu.CpuStateLock) {
                return new Z80CpuSnapshot {
                    A = cpu.A,
                    Flags = cpu.Flags,
                    
                    B = cpu.B,
                    C = cpu.C,
                    D = cpu.D,
                    E = cpu.E,
                    H = cpu.H,
                    L = cpu.L,
                    I = cpu.I,
                    R = cpu.R,

                    IX = cpu.IX,
                    IY = cpu.IY,
                    PC = cpu.PC,
                    SP = cpu.SP,
                    AF_ = cpu.AF_,
                    BC_ = cpu.BC_,
                    DE_ = cpu.DE_,
                    HL_ = cpu.HL_,
                    NewInstruction = cpu.NewInstruction
                };
            }
        }
    }
}