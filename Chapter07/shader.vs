#version 330
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTexture;
out vec2 texCoord;

void main()
{
	gl_Position = vec4(aPos, 1.0);
	texCoord = aTexture;
}