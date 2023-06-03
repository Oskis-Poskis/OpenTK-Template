#version 330 core
layout(location = 0) in vec2 aPosition;

uniform vec3 topbar_color = vec3(0.2);
uniform vec3 background_color = vec3(0.15);
uniform vec3 border_color = vec3(1, 0, 0);

uniform float index = 0.0;

void main()
{
    gl_Position = vec4(aPosition.x, aPosition.y, -index, 1.0);
}