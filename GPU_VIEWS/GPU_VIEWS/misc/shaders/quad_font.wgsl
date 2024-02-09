// Vertex shader

struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) color: vec4<f32>,
    @location(2) tex_coords: vec2<f32>
};

struct VertexOutput {
    @builtin(position) clip_position: vec4<f32>,
    @location(0) tex_coords: vec2<f32>,
    @location(1) color: vec4<f32>
};

@group(2) @binding(0) var<uniform> data: DataUniform;

struct DataUniform{
    matrix: mat4x4<f32>
};

@vertex
fn vs_main(
    model: VertexInput) -> VertexOutput {
    var out: VertexOutput;

    let myMatrix: mat4x4<f32> = mat4x4<f32>(
    1.0, 0.0, 0.0, 0.0, // Column 1
    0.0, 1.0, 0.0, 0.0, // Column 2
    0.0, 0.0, 1.0, 0.0, // Column 3
    0.0, 0.0, 0.0, 1.0  // Column 4
);
    out.clip_position = myMatrix * vec4<f32>(model.position, 1.0);
    out.tex_coords = model.tex_coords;
    out.color = model.color;
    return out;
}
