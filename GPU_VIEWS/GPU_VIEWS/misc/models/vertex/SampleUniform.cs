using GPU_VIEWS.misc;
using Silk.NET.WebGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wgpu;
using WGPU_TEST.models.core.filters;

namespace GPU_VIEWS.misc
{
    
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct SampleUniform : IUniform
    {
    }
}
