using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class POP : LD_Generic<ushort>
    {
        public override string Mnemonic => "POP";
        public POP(Z80Cpu cpu, WideRegister register) : base(cpu, new RegAddrMode16Bit(cpu, register), new RegIndirectWide(cpu, WideRegister.SP, false), additionalM1TCycles: 0)
        {
        }

        public override void Clock()
        {
            base.Clock();

            if (base.IsComplete)
            {
                _cpu.SP += 2;
            }
        }
    }
}