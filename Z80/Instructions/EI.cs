namespace Z80.Instructions {
    public class EI : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "EI";

        public bool IsComplete => true;

        public EI(Z80Cpu cpu) {
            _cpu = cpu;
        }

        public void Clock()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
        }

        public void StartExecution()
        {
            _cpu.IFF1 = true;
            _cpu.IFF2 = true;
        }
    }
}