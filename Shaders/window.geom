#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

const vec3 topbar_color = vec3(0.2);
const vec3 background_color = vec3(0.15);
const vec3 border_color = vec3(0.075);

out vec3 triangle_color;
out vec2 uvs;
flat out int isMain;

void main()
{
    int id = gl_PrimitiveIDIn;

    isMain = int(id < 2);
    triangle_color = mix(background_color, mix(topbar_color, mix(border_color, vec3(1, 0, 0), step(12, id)), step(4, id)), step(2, id));

    uvs = vec2(0.0, 0.0);
    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    if (id == 0) uvs = vec2(0.0, 1.0);
    else if (id == 1) uvs = vec2(1.0, 1.0);
    gl_Position = gl_in[1].gl_Position;
    EmitVertex();

    if (id == 0) uvs = vec2(1.0, 1.0);
    else if (id == 1) uvs = vec2(1.0, 0.0);
    gl_Position = gl_in[2].gl_Position;
    EmitVertex();

    EndPrimitive();
}