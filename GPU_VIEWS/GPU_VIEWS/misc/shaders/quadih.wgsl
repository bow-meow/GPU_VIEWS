// Vertex shader

struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) tex_coords: vec2<f32>
};

struct VertexOutput {
    @builtin(position) clip_position: vec4<f32>,
    @location(0) tex_coords: vec2<f32>,
};

@group(2) @binding(0) var<uniform> data: DataUniform;

struct DataUniform{
    matrix: mat4x4<f32>
};

@vertex
fn vs_main(
    model: VertexInput) -> VertexOutput {
    var out: VertexOutput;

    out.clip_position = data.matrix * vec4<f32>(model.position, 1.0);
    out.tex_coords = model.tex_coords;
    return out;
}
