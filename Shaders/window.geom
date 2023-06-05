#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform vec3 topbar_color = vec3(0.2);
uniform vec3 background_color = vec3(0.15);
uniform vec3 border_color = vec3(0.075);

out vec3 color;

void main()
{
    int id = gl_PrimitiveIDIn;
    if (id <= 1) color = background_color;
    else if (id <= 3) color = topbar_color;
    else if (id <= 11) color = border_color;
    else color = vec3(1, 0, 0);

    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    EmitVertex();

    EndPrimitive();
}