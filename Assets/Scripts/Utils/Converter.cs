using System.Runtime.InteropServices;

namespace Utils
{
    public static class Converter
    {
        public static byte[] ToByteArray(object obj)
        {
            var size = Marshal.SizeOf(obj);
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static T FromByteArray<T>(byte[] data)
        {
            var size = data.Length;
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            var your_object = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return your_object;
        }
    }
}

