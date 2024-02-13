using System.Numerics;
using FontStashSharp.Interfaces;
using Silk.NET.Maths;

namespace GPU_VIEWS.misc;

public struct PosColTex
{
    public Vector3D<float> Position { get; set; }

    // public Vector4D<float> Color { get; set; }

    public Vector2D<float> TextureCoordinate { get; set; }
}
public static class SuperExt
{
    public static PosColTex ToMe(this VertexPositionColorTexture t)
    {
        return new PosColTex{ 
            Position = new Vector3D<float>(t.Position.X, t.Position.Y, t.Position.Z),
            //Color = new Vector4D<float>(t.Color.R, t.Color.G, t.Color.B, t.Color.A),
            TextureCoordinate = new Vector2D<float>(t.TextureCoordinate.X, t.TextureCoordinate.Y)};
    }
}