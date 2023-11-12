#version 460
out vec4 fragColor;

in vec2 texCoord;
in vec2 fragPos;

struct Camera
{
	vec3 lower_left_corner;
	vec3 horizontal;
	vec3 vertical;
	vec3 origin;
};

uniform Camera camera;

struct Ray
{
	vec3 origin;
	vec3 direction;
};

vec3 WorldTrace(Ray ray)
{
	vec3 dir = normalize(ray.direction);
	float t = (dir.y + 1.0) / 2.0;

	return mix(vec3(1.0), vec3(0.5, 0.7, 1.0), t);
}

void main()
{
	float u = texCoord.x;
	float v = texCoord.y;

	Ray ray;
	ray.origin = camera.origin;
	ray.direction = camera.lower_left_corner + u * camera.horizontal + v * camera.vertical - camera.origin;

	fragColor = vec4(WorldTrace(ray), 1.0);
}