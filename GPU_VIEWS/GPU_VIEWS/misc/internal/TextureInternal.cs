using Wgpu;

namespace GPU_VIEWS.misc
{
    public readonly struct TextureInternal
    {
        public TexturePtr Texture { get; }
        public TextureViewPtr View { get; }

        public TextureInternal(TexturePtr texture)
        {
            Texture = texture;
            View = Texture.CreateView();
        }
    }
}