using System;
namespace Z80.Instructions
{
    public class SetCarryFlag : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "SCF";

        public bool IsComplete => true;

        public SetCarryFlag(Z80Cpu cpu) 
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
            Z80Flags.HalfCarry_H.SetOrReset(_cpu, false);
            Z80Flags.AddSubtract_N.SetOrReset(_cpu, false);
            Z80Flags.Carry_C.SetOrReset(_cpu, true);
        }
    }
}