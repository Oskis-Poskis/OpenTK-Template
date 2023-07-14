#version 330 core

flat in int isMain;
in vec3 triangle_color;
in vec2 uvs;
out vec4 fragColor;

uniform vec3 window_shade;
uniform vec3 button_tint = vec3(1);
uniform int isButton;
uniform int hasTexture;

uniform sampler2D displaytexture;

void main()
{
    if (isButton < 1 && isMain == 1 && hasTexture == 1) fragColor = texture(displaytexture, uvs);
    else if (isButton < 1) fragColor = vec4(triangle_color * window_shade, 1.0);
    else fragColor = vec4(vec3(1, 0, 0) * window_shade * button_tint, 1.0);
}