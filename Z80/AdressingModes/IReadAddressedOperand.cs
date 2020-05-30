namespace Z80.AddressingModes
{
    public interface IReadAddressedOperand<T> : IClockable
    {
        T AddressedValue { get; }
    }
}