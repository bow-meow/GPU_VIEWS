using System;
using Silk.NET.Core.Native;
using System.Runtime.CompilerServices;

namespace GPU_VIEWS.Wgpu.Wgpu.MacOS
{
    internal unsafe struct ObjectiveCClass
    {
        public readonly nint NativePtr;

        public static implicit operator nint(ObjectiveCClass c)
        {
            return c.NativePtr;
        }

        public ObjectiveCClass(string name)
        {
            var namePtr = SilkMarshal.StringToPtr(name);
            NativePtr = ObjectiveCRuntime.objc_getClass(namePtr);
            SilkMarshal.Free(namePtr);
        }

        public T AllocInit<T>() where T : struct
        {
            var value = ObjectiveCRuntime.ptr_objc_msgSend(NativePtr, "alloc");
            ObjectiveCRuntime.objc_msgSend(value, "init");
            return Unsafe.AsRef<T>(&value);
        }
    }
}

