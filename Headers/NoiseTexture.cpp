#include "NoiseTexture.h"
NoiseTexture* NoiseTexture::noise_texture = new NoiseTexture();

NoiseTexture* NoiseTexture::GetInstance()
{
	return noise_texture;
}

unsigned int NoiseTexture::Perlin2D(int width /*= 128*/, int height /*= 128*/, float f /*= 4.0f*/, float s /*= 2.0f*/, bool is_smooth /*= true*/)
{
	data = std::make_unique < unsigned char[] >(width * height * 4);
	float x_pos = 1.0f / (static_cast<float>(width - 1));
	float y_pos = 1.0f / (static_cast<float>(height - 1));
	for (unsigned int row = 0; row < height; row++)
	{
		for (unsigned int col = 0; col < width; col++)
		{
			float x = static_cast<float>(col) * x_pos;
			float y = static_cast<float>(row) * y_pos;
			float sum = 0.0f;
			for (int oct = 0; oct < 4; oct++)
			{
				float freq = f * pow(2.0f, oct + 1);
				float scale = pow(s, oct + 1);

				glm::vec2 p(x * freq, y * freq);
				float value = glm::perlin(p, glm::vec2(freq)) / scale;
				sum += value;
				float result = (sum + 1.0f) / 2.0f;
				data[((row * width + col) * 4) + oct % 4] = static_cast<unsigned char> (255.0f * result);
			}
		}
	}

	return GenerateTex2D(width, height);
}

unsigned int NoiseTexture::GenerateTex2D(int width, int height)
{
	glGenTextures(1, &texture_id);
	glBindTexture(GL_TEXTURE_2D, texture_id);
	glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data.get());
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR_MIPMAP_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	glGenerateMipmap(GL_TEXTURE_2D);

	return texture_id;
}