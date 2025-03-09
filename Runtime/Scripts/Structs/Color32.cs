using System.Runtime.InteropServices;

namespace OscCore
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Color32 : IEquatable<Color32>
    {
        public byte a;
        public byte b;
        public byte g;
        public byte r;

        public readonly bool Equals(Color32 other)
            => a == other.a && b == other.b && g == other.g && r == other.r;
        public override readonly bool Equals(object? obj)
            => obj is Color32 color && Equals(color);
        public static bool operator ==(Color32 left, Color32 right)
            => left.Equals(right);
        public static bool operator !=(Color32 left, Color32 right)
            => !(left == right);
        public override readonly int GetHashCode()
            => HashCode.Combine(a, b, g, r);
    }
}