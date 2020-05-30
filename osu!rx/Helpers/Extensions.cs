using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace osu_rx.Helpers
{
    public static class Extensions
    {
        public static float NextFloat(this Random random, float min, float max) => (float)random.NextDouble() * (max - min) + min;

        public static bool AlmostEquals(this double d, double value, double allowance) => Math.Abs(d - value) <= allowance;

        public static bool AlmostEquals(this float f, float value, float allowance) => Math.Abs(f - value) <= allowance;

        public static Vector2 ToVector2(this Point point) => new Vector2(point.X, point.Y);

        public static float Clamp(this float value, float min, float max) => value < min ? min : value > max ? max : value;

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                    if (attribute != null)
                        return attribute.Description;
                }

                return name;
            }

            return string.Empty;
        }
    }
}
