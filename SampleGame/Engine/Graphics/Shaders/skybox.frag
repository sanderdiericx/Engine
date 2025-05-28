#version 330 core
in vec3 texCoord;
out vec4 outputColor;

uniform samplerCube skybox;

void main()
{
    outputColor = texture(skybox, texCoord);
}
