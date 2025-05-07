#version 330

out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;

uniform int useTexture;
uniform vec3 materialColor;

void main()
{
    if (useTexture == 1)
    {
        outputColor = texture(texture0, texCoord);
    }
    else
    {
        outputColor = vec4(materialColor, 1.0);
    }
}