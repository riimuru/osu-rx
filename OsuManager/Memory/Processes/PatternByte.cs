namespace osu.Memory.Processes
{
    public class PatternByte
    {
        public byte Byte;
        public bool IsWildcard;

        public PatternByte(byte patternByte, bool isWildcard = false)
        {
            Byte = patternByte;
            IsWildcard = isWildcard;
        }

        public bool Matches(byte b) => IsWildcard || Byte == b;
    }
}
