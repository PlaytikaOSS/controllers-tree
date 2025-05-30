#if UNITY_CONTROLLERS_PROFILER
using System;
using System.Runtime.InteropServices;

namespace Playtika.Controllers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedString
    {
        public unsafe fixed byte Data[128];

        public override unsafe string ToString()
        {
            fixed (byte* dataPtr = Data)
            {
                return GetStringFromFixedBytes(dataPtr);
            }
        }

        private unsafe void Set(string value)
        {
            fixed (byte* dataPtr = Data)
            {
                CopyToFixedBytes(value, dataPtr, 128);
            }
        }

        private static unsafe void CopyToFixedBytes(string source, byte* destination, int maxLength)
        {
            if (source == null)
            {
                destination[0] = 0;
                return;
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(source);
            var bytesToCopy = Math.Min(bytes.Length, maxLength - 1);

            for (int i = 0; i < bytesToCopy; i++)
            {
                destination[i] = bytes[i];
            }

            destination[bytesToCopy] = 0;
        }

        private static unsafe string GetStringFromFixedBytes(byte* source)
        {
            var byteLength = 0;
            while (source[byteLength] != 0)
            {
                byteLength++;
            }

            return System.Text.Encoding.UTF8.GetString(source, byteLength);
        }

        public static implicit operator FixedString(string value)
        {
            var result = new FixedString();
            result.Set(value);
            return result;
        }

        public static implicit operator string(FixedString value)
        {
            return value.ToString();
        }
    }
}
#endif