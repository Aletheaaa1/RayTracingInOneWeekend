#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/type_ptr.hpp>

#include <iostream>

#include "../Headers/Shader.h"
#include "../Headers/Geometries.h"

glm::vec3 origin = glm::vec3{ 0.0f, 0.0f, 0.0f };
glm::vec3 target = glm::vec3{ 0.0f, 0.0f, -1.0f };
glm::vec3 up = glm::vec3{ 0.0f, 1.0f, 0.0f };

float speed = 0.0001f;

constexpr int SCREEN_WIDTH = 800;
constexpr int SCREEN_HEIGHT = 400;

#pragma region Interaction
float x_last, y_last;
float theta = 0.0f, phi = 0.0f;
bool first_mouse = true;

void UpdatePosition(glm::vec3& position, glm::vec3& target, glm::vec3& up, float speed_x, float speed_y, float speed_z, GLFWwindow* window)
{
	double x_pos, y_pos;
	float view_speed = 0.4f;
	glfwGetCursorPos(window, &x_pos, &y_pos);

	if (first_mouse == true)
	{
		x_last = x_pos;
		y_last = y_pos;
		first_mouse = false;
	}

	float x_offset = x_pos - x_last;
	float y_offset = y_pos - y_last;

	theta += x_offset / SCREEN_WIDTH * 360.0f * view_speed;
	phi += y_offset / SCREEN_HEIGHT * 180.0f * view_speed;

	if (phi > 90.0f)
	{
		phi = 90.0f;
	}
	else if (phi < -90.0f)
	{
		phi = -90.0f;
	}

	float y = sin(glm::radians(phi));
	float x = cos(glm::radians(phi)) * cos(glm::radians(theta));
	float z = cos(glm::radians(phi)) * sin(glm::radians(theta));

	target += glm::vec3(x, -y, z);

	glm::vec3 forword = glm::normalize(position - target);

	position += forword * speed_z;
	position += glm::normalize(glm::cross(up, forword)) * speed_x;
	position += up * speed_y;
	target = position - forword;

	x_last = x_pos;
	y_last = y_pos;
}

void ProcessInput(GLFWwindow* window)
{
	float speed = 0.2f;
	float speed_x = 0.0f;
	float speed_y = 0.0f;
	float speed_z = 0.0f;
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
	{
		glfwSetWindowShouldClose(window, true);
	}
	if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
	{
		speed_z -= speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
	{
		speed_z += speed;
	}
	else
	{
		speed_z = 0.0f;
	}
	if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
	{
		speed_x -= speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
	{
		speed_x += speed;
	}
	else
	{
		speed_x = 0.0f;
	}
	if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS)
	{
		speed_y += speed;
	}
	else if (glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == GLFW_PRESS)
	{
		speed_y -= speed;
	}
	else
	{
		speed_y = 0.0f;
	}

	UpdatePosition(origin, target, up, speed_x, speed_y, speed_z, window);
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
	window = glfwCreateWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Hello World", nullptr, nullptr);
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
	//glfwSetCursorPosCallback(window, MouseCallback);

#pragma region Configue
	Shader shader{ "./Chapter07/shader.vs", "./Chapter07/shader.fs" };
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

		shader.SetUniform2v("screenSize", glm::vec2{ SCREEN_WIDTH, SCREEN_HEIGHT });
		shader.SetUniform3v("cameraPos", origin);
		shader.SetUniform3v("cameraTarget", target);
		shader.SetUniform3v("cameraUp", up);

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
		}
	}

	glfwTerminate();
	return 0;
}