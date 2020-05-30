namespace Z80
{
    public interface IClockable
    {
        void Clock();

        void Reset();

        bool IsComplete { get; }
    }
}