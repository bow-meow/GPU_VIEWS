using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Silk.NET.WebGPU;
using Wgpu;
using WGPU_TEST;
using wgpu = Wgpu;

namespace GPU_VIEWS.misc
{
    public class ShaderFactory
    {
        private readonly IDictionary<ShaderType, ShaderModulePtr> _cache = new Dictionary<ShaderType, ShaderModulePtr>();

        public ShaderModulePtr GetOrCreate(DevicePtr device, ShaderType shaderType)
        {
            if(_cache.ContainsKey(shaderType))
                return _cache[shaderType];

            var type = shaderType.GetType();

            var memberInfo = type.GetMember(shaderType.ToString());

            var attribute = (ResourceAttr)Attribute.GetCustomAttribute(memberInfo[0], typeof(ResourceAttr));

            if(attribute == null)
                throw new System.Exception($"shader {shaderType} has no attrubute path");

            string wgsl = string.Empty;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(attribute.EmbedPath))
            using (var sr = new StreamReader(stream))
                wgsl = sr.ReadToEnd();

            var module = device.CreateShaderModuleWGSL(wgsl, new wgpu.ShaderModuleCompilationHint[]
            {
                new wgpu.ShaderModuleCompilationHint { EntryPoint = "vs_main"},
                new wgpu.ShaderModuleCompilationHint { EntryPoint = "fs_main"}
            });

            _cache[shaderType] = module;

            return module;
        }
    }
}