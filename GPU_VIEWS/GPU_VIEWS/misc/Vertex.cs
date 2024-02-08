using Silk.NET.Maths;

namespace GPU_VIEWS.misc
{
    public readonly struct Vertex
    {
        public Vertex(Vector3D<float> position, Vector2D<float> textureCoord)
        {
            Position = position;
            TextureCoords = textureCoord;
        }

        public Vector3D<float> Position { get; }
        public Vector2D<float> TextureCoords { get; }

        public static (Vertex[] quad, uint[] indexMap) GetFullScreenQuad() =>
        (
            new[] 
            {
                // Positions  // Texture Coords
                new Vertex(new Vector3D<float>(1.0f, 1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f - 1.0f)),
                new Vertex(new Vector3D<float>(-1.0f, 1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f - 1.0f)),
                new Vertex(new Vector3D<float>(-1.0f, -1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f - 0.0f)),
                new Vertex(new Vector3D<float>(1.0f, -1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f - 0.0f))
            },
            new uint[]
            {
                0, 1, 2,
                3, 0, 2,
            }
        );
    };
}
