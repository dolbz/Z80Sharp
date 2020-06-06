using System;
namespace Z80.Instructions
{
    public class ComplementCarryFlag : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "CCF";

        public bool IsComplete => true;

        public ComplementCarryFlag(Z80Cpu cpu) 
        {
            _cpu = cpu;
        }

        public void Clock()
        {
            throw new InvalidOperationException();
        }

        public void Reset()
        {
        }

        public void StartExecution()
        {
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, true);
            Z80Flags.Carry_C.SetOrReset(_cpu, !_cpu.Flags.HasFlag(Z80Flags.Carry_C));
        }
    }
}