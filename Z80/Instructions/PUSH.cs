using Z80.AddressingModes;

namespace Z80.Instructions
{
    internal class PUSH : LD_Generic<ushort>
    {
        public PUSH(Z80Cpu cpu, WideRegister register) : base(cpu, new RegIndirectWide(cpu, WideRegister.SP, true), new RegAddrMode16Bit(cpu, register), additionalM1TCycles: 1)
        {

        }

        public override void Clock()
        {
            base.Clock();

            if (IsComplete)
            {
                _cpu.SP -= 2;
            }
        }
    }
}