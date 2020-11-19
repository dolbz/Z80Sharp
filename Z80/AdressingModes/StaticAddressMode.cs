namespace Z80.AddressingModes {
    public class StaticAddressMode : IAddressMode<ushort>, IReadAddressedOperand<ushort>
    {
        private ushort _addressedValue;

        public IReadAddressedOperand<ushort> Reader => this;

        public IWriteAddressedOperand<ushort> Writer => throw new System.NotImplementedException("Cannot write with static address mode");

        public StaticAddressMode(ushort addressedValue) {
            _addressedValue = addressedValue;
        }
        public bool IsComplete => true;

        public ushort AddressedValue => _addressedValue;

        public string Description => $"{AddressedValue}";

        public void Clock()
        {
        }

        public void Reset()
        {
        }
    }
}