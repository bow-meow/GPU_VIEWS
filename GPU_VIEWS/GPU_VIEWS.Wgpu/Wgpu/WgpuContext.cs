using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wgpu
{
	public static class WgpuContext
	{
		public static WebGPU CreateWgpuContext()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return new WebGPU(WebGPU.CreateDefaultContext("libwgpu_native.dylib"));
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return new WebGPU(WebGPU.CreateDefaultContext(new[] { "wgpu_native.dll" }));
			}
			else
				throw new Exception();
		}
	}
}
