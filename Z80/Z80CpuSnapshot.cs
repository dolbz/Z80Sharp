using System.Runtime.CompilerServices;
namespace Z80 {
    public struct Z80CpuSnapshot {
        public byte A { get; private set; }
        public readonly Z80Flags Flags;
        public readonly byte B;
        public readonly byte C;
        public readonly byte D;
        public readonly byte E;
        public readonly byte H;
        public readonly byte L;
        public readonly ushort AF_;
        public readonly ushort BC_;
        public readonly ushort DE_;
        public readonly ushort HL_;

        public readonly byte I;
        public readonly byte R;
        public readonly ushort IX;
        public readonly ushort IY;
        public readonly ushort SP;

        public readonly ushort PC;

        public readonly ushort Address;
        public readonly byte Data;

        public readonly bool MREQ;

        public readonly bool IORQ;

        public readonly bool RD;

        public readonly bool WR;

        public readonly bool M1;

        public readonly bool RFRSH;

        internal static Z80CpuSnapshot FromCpu(Z80Cpu cpu) {
            lock (cpu.CpuStateLock) {
                return new Z80CpuSnapshot {
                    A = cpu.A
                    // TODO all the other fields
                };
            }
        }
    }
}