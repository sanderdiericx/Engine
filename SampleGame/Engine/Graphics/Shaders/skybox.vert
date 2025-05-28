#version 330 core
layout (location = 0) in vec3 aPosition;

out vec3 texCoord;

uniform mat4 view;
uniform mat4 projection;

void main()
{
    texCoord = aPosition;
    vec4 pos = projection * view * vec4(aPosition, 1.0);
    gl_Position = pos.xyww; // Keeps skybox at depth 1
}