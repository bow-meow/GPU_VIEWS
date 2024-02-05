
namespace Wgpu
{
#pragma warning disable IDE1006 // Naming Styles
    public interface GPUError
#pragma warning restore IDE1006 // Naming Styles
    {
        public string Message { get; }
    }

    public record ValidationError(string Message) : GPUError;
    public record OutOfMemoryError(string Message) : GPUError;
    public record InternalError(string Message) : GPUError;
}
