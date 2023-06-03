#version 330 core
layout(location = 0) in vec2 aPosition;
out vec3 color;
out vec2 UV;

uniform vec3 topbar_color = vec3(0.2);
uniform vec3 background_color = vec3(0.15);
uniform float index = 0.0;

void main()
{
    gl_Position = vec4(aPosition.x, aPosition.y, -index, 1.0);

    UV = aPosition * 0.5 + 0.5;

    vec3 temp = topbar_color;
    color = background_color;
}