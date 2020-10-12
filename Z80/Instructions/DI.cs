namespace Z80.Instructions {
    public class DI : IInstruction
    {
        private Z80Cpu _cpu;
        public string Mnemonic => "DI";

        public bool IsComplete => true;

        public DI(Z80Cpu cpu) {
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
            _cpu.IFF1 = false;
            _cpu.IFF2 = false;
        }
    }
}