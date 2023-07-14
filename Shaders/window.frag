#version 330 core

in vec3 triangle_color;
out vec4 fragColor;

uniform vec3 window_shade;
uniform vec3 button_tint = vec3(1);
uniform int isButton = 0;

void main()
{
    if (isButton < 1) fragColor = vec4(triangle_color * window_shade, 1.0);
    else fragColor = vec4(vec3(1, 0, 0) * window_shade * button_tint, 1.0);
}