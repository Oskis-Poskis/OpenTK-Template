#version 330 core
in vec3 color;
out vec4 fragColor;

uniform vec3 shade;
uniform int interaction = 0;
void main()
{
    if (interaction < 1) fragColor = vec4(color * shade, 1.0);
    else fragColor = vec4(vec3(1, 0, 0), 1.0);
}