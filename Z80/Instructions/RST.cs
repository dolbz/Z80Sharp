using Z80.AddressingModes;

namespace Z80.Instructions {
    internal class RST : CALL {
        public override string Mnemonic => "RST";
        public RST(Z80Cpu cpu, ushort jumpValue) : base(cpu, new StaticAddressMode(jumpValue), JumpCondition.Unconditional) {

        }
    }
}