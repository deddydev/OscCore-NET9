using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BlobHandles
{
    /// <summary>
    /// Represents a string as a fixed blob of bytes
    /// </summary>
    public struct BlobString : IDisposable, IEquatable<BlobString>
    {
        /// <summary>
        /// The encoding used to convert to and from strings.
        /// WARNING - Changing this after strings have been encoded will probably lead to errors!
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.ASCII;

        public readonly BlobHandle Handle;
        private readonly bool m_OwnsMemory;

        public readonly int Length => Handle.Length;

        public unsafe BlobString(string source)
        {
            var byteCount = Encoding.GetByteCount(source);
            var nativeBytesPtr = (byte*)Marshal.AllocHGlobal(byteCount);

            // write encoded string bytes directly to unmanaged memory
            fixed (char* strPtr = source)
            {
                Encoding.GetBytes(strPtr, source.Length, nativeBytesPtr, byteCount);
                Handle = new BlobHandle(nativeBytesPtr, byteCount);
            }
            m_OwnsMemory = true;
        }

        public unsafe BlobString(byte* sourcePtr, int length)
        {
            Handle = new BlobHandle(sourcePtr, length);
            m_OwnsMemory = false;
        }

        public override readonly unsafe string ToString()
            => Encoding.GetString(Handle.Pointer, Handle.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => Handle.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BlobString other)
            => Handle.Equals(other.Handle);

        public override readonly bool Equals(object? obj)
            => obj is BlobString other && Handle.Equals(other.Handle);

        public static bool operator ==(BlobString l, BlobString r)
            => l.Handle == r.Handle;

        public static bool operator !=(BlobString l, BlobString r)
            => l.Handle != r.Handle;

        public readonly unsafe void Dispose()
        {
            if (m_OwnsMemory)
                Marshal.FreeHGlobal((nint)Handle.Pointer);
        }
    }
}
