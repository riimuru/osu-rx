namespace osu_rx.Helpers
{
    public class MathHelper
    {
        public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
