#version 460

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

out vec4 fragColor;

// -------------------------------------

struct Ray
{
	vec3 origin;
	vec3 direction;
};

Ray NewRay(vec3 origin, vec3 direction)
{
	Ray ray;
	ray.origin = origin;
	ray.direction = direction;
	return ray;
}

struct Sphere
{
	vec3 center;
	float radius;
};

Sphere NewSphere(vec3 center, float radius)
{
	Sphere sphere;
	sphere.center = center;
	sphere.radius = radius;
	return sphere;
}

// -------------------------------------

//	计算光线交点
vec3 RayGetPointAt(Ray ray, float t)
{
	return ray.origin + t * ray.direction;
}

//	判断光线与球体相交
float SphereHit(Sphere sphere, Ray ray)
{
	vec3 oc = ray.origin - sphere.center;

	float a = dot(ray.direction, ray.direction);
	float b = 2.0 * dot(oc, ray.direction);
	float c = dot(oc, oc) - sphere.radius * sphere.radius;

	float discriminant = b*b - 4*a*c;

	if(discriminant < 0.0)
	{
		return -1;
	}
	else
	{
		float intersection = min(abs((-b - sqrt(discriminant)) / (2.0 * a)), abs((-b + sqrt(discriminant)) / (2.0 * a)));
		return intersection;
	}

}

//	光线追踪
vec3 WorldTrace(Ray ray)
{
	Sphere sphere = NewSphere(vec3(0.0, 0.0, -1.0), 0.5);

	float t = SphereHit(sphere, ray);
	if(t > 0.0)
	{
		//	法线
		vec3 N = normalize(RayGetPointAt(ray, t) - sphere.center);
		return 0.5 * (N + 1.0);
	}
	else
	{
		vec3 dir = normalize(ray.direction);
		float t = (dir.y + 1.0) / 2.0;
		return mix(vec3(1.0), vec3(0.5, 0.7, 1.0), t);
	}
}

void main()
{
	float u = fragPos.x;
	float v = fragPos.y;

	Ray ray;
	ray.origin = camera.origin;
	ray.direction = camera.lower_left_corner + u * camera.horizontal + v * camera.vertical - camera.origin;

	fragColor = vec4(WorldTrace(ray), 1.0);
}