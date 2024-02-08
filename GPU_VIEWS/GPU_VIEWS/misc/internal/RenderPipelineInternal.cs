using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly struct RenderPiplineInternal
    {
        public PipelineLayoutPtr PipelineLayout { get; }
        public RenderPipelinePtr RenderPipeline { get; }

        public RenderPiplineInternal(PipelineLayoutPtr pipelineLayout, 
        RenderPipelinePtr renderPipeline)
        {
            PipelineLayout = pipelineLayout;
            RenderPipeline = renderPipeline;
        }
    }
}