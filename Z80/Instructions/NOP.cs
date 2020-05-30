namespace Z80.Instructions
{
    internal class NOP : IInstruction
    {

        public string Mnemonic => "NOP";

        public bool IsComplete => true;

        public void StartExecution()
        {
            // No operation
        }
        public void Clock()
        {
            // Do no operation
        }

        public void Reset()
        {
            // Nothing to do
        }
    }
}