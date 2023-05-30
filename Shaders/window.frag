#version 330 core
out vec4 fragColor;

in vec2 color;

void main()
{
    fragColor = vec4(color, 0.0, 1.0);
}