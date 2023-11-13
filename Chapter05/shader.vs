#version 460
layout (location = 0) in vec3 aPos;
layout (location = 2) in vec2 aTexture;

out vec2 screenCoord;
out vec2 texCoord;

void main()
{
	gl_Position = vec4(aPos, 1.0);
	screenCoord = (vec2(aPos.x, aPos.y) + 1.0) / 2.0;
	texCoord = aTexture;
}