using System;
using WGPU_TEST;

namespace GPU_VIEWS.misc
{
    public class ResourceAttr : Attribute
    {
        public ResourceAttr(string embedPath)
        {
            EmbedPath = embedPath;
        }

        public string EmbedPath { get; }
    }
}