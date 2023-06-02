#version 330 core
out vec4 fragColor;

uniform vec3 col = vec3(0.5);

void main()
{
    fragColor = vec4(col, 1.0);
}