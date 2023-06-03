#version 330 core
in vec3 color;
in vec2 UV;
out vec4 fragColor;

uniform vec3 col;

void main()
{
    fragColor = vec4(vec3(color * col), 1.0);
}