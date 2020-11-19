namespace Z80.AddressingModes
{
    public interface IAddressMode<T> : IClockable {
        string Description { get; }
        IReadAddressedOperand<T> Reader { get; }

        IWriteAddressedOperand<T> Writer {get; }
    }
}