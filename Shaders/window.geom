#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

const vec3 topbar_color = vec3(0.2);
const vec3 background_color = vec3(0.15);
const vec3 border_color = vec3(0.075);

out vec3 triangle_color;

void main()
{
    int id = gl_PrimitiveIDIn;
    if (id < 2) triangle_color = background_color;   // Main content
    else if (id < 4) triangle_color = topbar_color;  // Top bar
    else if (id < 12) triangle_color = border_color; // Borders
    else triangle_color = vec3(1, 0, 0);             // Collapse button

    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    EmitVertex();

    EndPrimitive();
}