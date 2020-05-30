namespace Z80.AddressingModes
{
    public interface IWriteAddressedOperand<T> : IClockable
    {
        T AddressedValue { set; }
    }
}