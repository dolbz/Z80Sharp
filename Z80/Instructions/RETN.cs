namespace Z80.Instructions {
    internal class RETN : RET {
        public RETN(Z80Cpu cpu) : base(cpu, JumpCondition.Unconditional) {

        }

        public override string Mnemonic => "RETN";
        public override void Clock()
        {
            base.Clock();
            if (IsComplete) {
                _cpu.IFF1 = _cpu.IFF2;
            }
        }
    }
}