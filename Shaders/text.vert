#version 330 core
layout(location = 0) in vec2 aPosition;

uniform float index = 0.0;
void main()
{
    gl_Position = vec4(aPosition.x, aPosition.y, 0.0, 1.0);
}