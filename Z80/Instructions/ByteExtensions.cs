namespace Z80.Instructions
{
    internal static class ByteExtensions
    {
        public static bool IsEvenParity(this byte value)
        {
            value ^= (byte)(value >> 4);
            value ^= (byte)(value >> 2);
            value ^= (byte)(value >> 1);
            return (value & 0x1) == 0;
        }
    }
}