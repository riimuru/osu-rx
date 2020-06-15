namespace osu.Memory
{
    public static class Signatures
    {
        public static readonly Signature Time = new Signature
        {
            Pattern = "7E 55 8B 76 10 DB 05",
            Offset = 0x7
        };

        public const int IsAudioPlayingOffset = 0x30;

        public static readonly Signature Mode = new Signature
        {
            Pattern = "8D 45 BC 89 46 0C 83 3D",
            Offset = 0x8
        };

        public static readonly Signature Viewport = new Signature
        {
            Pattern = "56 FF 75 F0 8B 15",
            Offset = 0x6
        };

        public static readonly Signature BindingManager = new Signature
        {
            Pattern = "8B 3D ?? ?? ?? ?? 38 07 8B CF",
            Offset = 0x2
        };

        public static readonly Signature Player = new Signature
        {
            Pattern = "FF 50 0C 8B D8 8B 15",
            Offset = 0x7
        };
    }
}
