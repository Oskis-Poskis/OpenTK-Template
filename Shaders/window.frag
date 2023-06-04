#version 330 core
in vec3 color;
in vec2 UV;
out vec4 fragColor;

uniform vec3 shade;
uniform int button;

void main()
{
    if (button == 0) fragColor = vec4(color * shade, 1.0);
    else fragColor = vec4(vec3(1), 1.0); 
}