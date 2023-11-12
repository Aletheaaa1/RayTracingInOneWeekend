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

struct World
{
	int objectNumber;
	Sphere objects[10];
};

World NewWorld()
{
	World world;
	world.objectNumber = 2;
	world.objects[0] = NewSphere(vec3(0.0, 0.0, -1.0), 0.5);
	world.objects[1] = NewSphere(vec3(0.0, -100.5, -1.0), 100.0);

	return world;
}

struct HitRecord
{
	float t;
	vec3 position;
	vec3 normal;
};

// -------------------------------------

//	随机数
float RandomFloat (float x) {
   return fract(sin(dot(100 * x, 78.233))* 43758.5453123);
}

vec2 Random(vec2 uv)
{
	return vec2(RandomFloat(uv.x), RandomFloat(uv.y));
}

//	计算光线交点
vec3 RayGetPointAt(Ray ray, float t)
{
	return ray.origin + t * ray.direction;
}

//	判断光线与球体相交
bool SphereHit(Sphere sphere, Ray ray, float t_min, float t_max, inout HitRecord rec)
{
	vec3 oc = ray.origin - sphere.center;

	float a = dot(ray.direction, ray.direction);
	float b = 2.0 * dot(oc, ray.direction);
	float c = dot(oc, oc) - sphere.radius * sphere.radius;

	float discriminant = b*b - 4*a*c;

	if(discriminant > 0.0)
	{
		float temp = (-b - sqrt(discriminant)) / (2.0 * a);
		if(temp < t_max && temp > t_min)
		{
			rec.t = temp;
			rec.position = RayGetPointAt(ray, rec.t);
			rec.normal = normalize(rec.position - sphere.center);

			return true;
		}

		temp = (-b + sqrt(discriminant)) / (2.0 * a);
		if(temp < t_max && temp > t_min)
		{
			rec.t = temp;
			rec.position = RayGetPointAt(ray, rec.t);
			rec.normal = normalize(rec.position - sphere.center);

			return true;
		}
	}

	return false;
}

//	记录world中所有光线相交物体信息
bool WorldHit(World world, Ray ray, float t_min, float t_max, inout HitRecord rec)
{
	HitRecord temRec;
	float cloestSoFar = t_max;
	bool hitSomething = false;

	for(int i=0; i<world.objectNumber; i++)
	{
		if(SphereHit(world.objects[i], ray, t_min, cloestSoFar, temRec))
		{
			rec = temRec;
			cloestSoFar = rec.t;
			hitSomething = true;
		}
	}

	return hitSomething;
}

//	光线追踪
vec3 WorldTrace(Ray ray, World world)
{
	HitRecord hitRecord;
	if(WorldHit(world, ray, 0.0, 1000000.0, hitRecord))
	{
		//	法线
		vec3 N = (hitRecord.normal + 1.0) / 2.0;
		return 0.5 * (N + 1.0);
	}
	else
	{
		vec3 dir = normalize(ray.direction);
		float t = (dir.y + 1.0) / 2.0;
		return mix(vec3(1.0), vec3(0.5, 0.7, 1.0), t);
	}
}

Ray AARay(Camera camera, vec2 offset)
{
	Ray ray = NewRay(camera.origin,
		camera.lower_left_corner +
		offset.x * camera.horizontal +
		offset.y * camera.vertical -
		camera.origin);

	return ray;
}

//uint m_u = uint(521288629);
//uint m_v = uint(362436069);
//
//uint GetUintCore(inout uint u, inout uint v)
//{
//	v = uint(uint(36969) * (v & uint(65535)) + (v >> 16));
//	u = uint(uint(18000) * (u & uint(65535)) + (u >> 16));
//	return (v << 16) + u;
//}
//
//float GetUniformCore(inout uint u, inout uint v)
//{
//	// 0 <= u <= 2^32
//	uint z = GetUintCore(u, v);
//	// The magic number is 1/(2^32 + 1) and so result is positive and less than 1.
//	return float(z) / uint(4294967295);
//}
//
//float GetUniform()
//{
//	return GetUniformCore(m_u, m_v);
//}
//
//unsigned int GetUint()
//{
//	return GetUintCore(m_u, m_v);
//}
//
//float rand()
//{
//	return GetUniform();
//}
//
//vec2 rand2()
//{
//	return vec2(rand(), rand());
//}

void main()
{
	float u = fragPos.x;
	float v = fragPos.y;

	World world = NewWorld();

	int samplerNumber = 100;
	vec3 col = vec3(0.0);

	vec2 texSize = 1.0 / vec2(800, 600);
	for(int i=-1; i<=1; i++)
	{
		for(int j=-1; j<=1; j++)
		{
			Ray ray = AARay(camera, texCoord + vec2(i, j) * texSize * 0.5);
			col += WorldTrace(ray, world);
		}
	}
	col /= 9.0;
//for(int i=0; i<samplerNumber; i++)
//	{
//		Ray ray = AARay(camera, texCoord + rand2() /  vec2(800, 600));
//		col += WorldTrace(ray, world );
//	}
//	col /= samplerNumber;

	fragColor = vec4(col, 1.0);
}