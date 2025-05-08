#version 330 core
layout (location = 0) in vec3 aPos;
laytout (location = 1) in vec2 aTexCoord;

out vec3 texCoord;

uniform mat4 view;
uniform mat4 projection;

void main()
{
    vec4 pos = projection * view * vec4(aPos, 1.0);
    gl_Position = pos.xyww; // Keeps skybox at depth 1
    texCoord = aTexCoord;
}