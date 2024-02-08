using System;
using Silk.NET.WebGPU;

namespace GPU_VIEWS.misc
{
    public readonly ref struct BindGroupLayoutDesc
    {
        public ReadOnlySpan<BindGroupLayoutEntry> Entries { get; }

        public BindGroupLayoutDesc(ReadOnlySpan<BindGroupLayoutEntry> entries)
        {
            Entries = entries;
        }
    }
}