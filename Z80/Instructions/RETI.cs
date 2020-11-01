namespace Z80.Instructions {
    internal class RETI : RET {
        public RETI(Z80Cpu cpu) : base(cpu, JumpCondition.Unconditional) {

        }
        
        public override string Mnemonic => "RETI";
    }
}