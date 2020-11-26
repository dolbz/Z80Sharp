namespace Z80.Instructions.InterruptHandlers {
    internal class Mode1Handler : RST
    {
        public Mode1Handler(Z80Cpu cpu) : base(cpu, 0x38) {
        }
    }
}