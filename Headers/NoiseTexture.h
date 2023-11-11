#pragma once
#include <glm/gtc/noise.hpp>
#include <glad/glad.h>
#include <memory>
#include <iostream>

class NoiseTexture
{
private:
	NoiseTexture(const NoiseTexture&) = delete;
	NoiseTexture& operator=(const NoiseTexture&) = delete;
	NoiseTexture() = default;

	unsigned int GenerateTex2D(int width, int height);

public:
	static NoiseTexture* GetInstance();
	unsigned int Perlin2D(int width = 128, int height = 128, float freq = 4.0f, float scale = 2.0f, bool is_smooth = true);

private:
	std::unique_ptr<unsigned char[]> data;
	unsigned int texture_id = 0;

public:
	static	NoiseTexture* noise_texture;
};
