namespace Z80.AddressingModes
{
    public interface IAddressMode<T> : IClockable {
        IReadAddressedOperand<T> Reader { get; }

        IWriteAddressedOperand<T> Writer {get; }
    }
}