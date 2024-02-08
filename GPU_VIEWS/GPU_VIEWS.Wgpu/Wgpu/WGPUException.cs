
using System;

namespace Wgpu
{
    public class WGPUException : Exception
    {
        public WGPUException(string message) : base(message) { }
    }
}
