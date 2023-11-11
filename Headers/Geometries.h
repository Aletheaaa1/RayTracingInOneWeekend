#pragma once
#include <glad/glad.h>
#include <glm/glm.hpp>

#include "./VertexArrayObject.h";
#include "./VertexBufferObject.h"
#include "./Shader.h"

class Geometries
{
public:
	Geometries() = delete;
	static void RenderQuad();
	static void RenderCube();
};
