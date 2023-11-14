#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <random>

#include <iostream>

#include "../Headers/Model.h"
#include "../Headers/Shader.h"
#include "../Headers/Texture.h"
#include "../Headers/Camera.h"
#include "../Headers/Geometries.h"

Camera camera{ glm::vec3(0, 0.0f, 0.0f), 0.0f, 0.0f, glm::vec3(0, 1.0f, 0) };
float speed = 0.0001f;

constexpr int SCREEN_WIDTH = 800;
constexpr int SCREEN_HEIGHT = 600;

#pragma region Interaction
float x_last, y_last;
bool first_mouse = true;
void MouseCallback(GLFWwindow* window, double x_pos, double y_pos)
{
	if (first_mouse == true)
	{
		x_last = x_pos;
		y_last = y_pos;
		first_mouse = false;
	}

	float x_offset = x_pos - x_last;
	float y_offset = y_pos - y_last;

	x_last = x_pos;
	y_last = y_pos;

	camera.UpdateCameraView(x_offset, y_offset);
	camera.GetViewMatrix();
}

void ProcessInput(GLFWwindow* window)
{
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
	{
		glfwSetWindowShouldClose(window, true);
	}
	if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
	{
		camera.speedZ = -speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
	{
		camera.speedZ = speed;
	}
	else
	{
		camera.speedZ = 0.0f;
	}
	if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
	{
		camera.speedX = -speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
	{
		camera.speedX = speed;
	}
	else
	{
		camera.speedX = 0.0f;
	}
	if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS)
	{
		camera.speedY = speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == GLFW_PRESS)
	{
		camera.speedY = -speed;
	}
	else
	{
		camera.speedY = 0.0f;
	}
}
#pragma endregion Interaction

void RenderQuad();

int main()
{
	GLFWwindow* window;

	/* Initialize the library */
	if (!glfwInit())
		return -1;

	/* Create a windowed mode window and its OpenGL context */
	window = glfwCreateWindow(800, 600, "Hello World", nullptr, nullptr);
	if (!window)
	{
		glfwTerminate();
		return -1;
	}

	/* Make the window's context current */
	glfwMakeContextCurrent(window);

	if (!gladLoadGL())
	{
		std::cout << "glad loading error \n";
	}

	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 6);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	glfwWindowHint(GLFW_RESIZABLE, false);
	glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
	glfwSetCursorPosCallback(window, MouseCallback);

#pragma region Configue
	Shader shader{ "./Chapter06/shader.vs", "./Chapter06/shader.fs" };
#pragma endregion Configue

	float fps = 0;
	double last_time = glfwGetTime();
	/* Loop until the user closes the window */
	while (!glfwWindowShouldClose(window))
	{
		glfwSwapInterval(0);

		const double current_time = glfwGetTime();
		fps++;
		if (current_time - last_time >= 1.0)
		{
			glfwSetWindowTitle(window, std::to_string(static_cast<int>(fps)).c_str());
			fps = 0.0;
			last_time += 1.0;
		}

		/* Render here */
		glClear(GL_COLOR_BUFFER_BIT);

		shader.Bind();
		shader.SetUniform3v("camera.lower_left_corner", glm::vec3{ -2.0f, -1.5f, -1.0f });
		shader.SetUniform3v("camera.horizontal", glm::vec3{ 4.0f, 0.0f, 0.0f });
		shader.SetUniform3v("camera.vertical", glm::vec3{ 0.0f, 3.0f, 0.0f });
		shader.SetUniform3v("camera.origin", glm::vec3{ 0.0f, 0.0f, 0.0f });

		Geometries::RenderCube();

		/* Swap front and back buffers */
		glfwSwapBuffers(window);

		/* Poll for and process events */
		glfwPollEvents();

		const double delta_time = current_time - last_time;
		constexpr float time_step = 1.0 / 144.0;
		if (delta_time >= time_step)
		{
			speed = 0.00005;
			ProcessInput(window);
			camera.UpdateCameraPosition();
		}
	}

	glfwTerminate();
	return 0;
}