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

//	材质
const int MAT_LAMBERTIAN = 0;
const int MAT_METALLIC = 1;
const int MAT_DIELECTRIC = 2;
const int MAT_PBR = 3;

struct Lambertian
{
	vec3 albedo;
};
struct Metallic
{
	vec3 albedo;
	float roughness;
};
struct Dielectric
{
	vec3 albedo;
	//	折色率
    float ior;
	float roughness;
};

Lambertian lambertMaterials[4];
Metallic metallicMaterials[4];
Dielectric dielectricMaterials[4];

Lambertian LambertianConstructor(vec3 albedo)
{
	Lambertian lambertian;

	lambertian.albedo = albedo;

	return lambertian;
}

Metallic MetallicConstructor(vec3 albedo, float roughness)
{
	Metallic metallic;

	metallic.albedo = albedo;
	metallic.roughness = roughness;

	return metallic;
}

Dielectric DielectricConstructor(vec3 albedo, float roughness, float ior)
{
	Dielectric dielectric;

	dielectric.albedo = albedo;
	dielectric.roughness = roughness;
	dielectric.ior = ior;

	return dielectric;
}
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
	//	材质
	int materialType;
	int material;
};

Sphere NewSphere(vec3 center, float radius, int materialType, int material)
{
	Sphere sphere;
	sphere.center = center;
	sphere.radius = radius;
	sphere.materialType = materialType;
	sphere.material = material;

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
	world.objects[0] = NewSphere(vec3(0.0, 0.0, -1.0), 0.5, MAT_LAMBERTIAN, 0);
	world.objects[1] = NewSphere(vec3(0.0, -100.5, -1.0), 100.0, MAT_LAMBERTIAN, 3);

	return world;
}

struct HitRecord
{
	float t;
	vec3 position;
	vec3 normal;
	int materialType;
	int material;
};

////////////////////////////////////////////////////
//	随机数
const float PI = 3.1415926;

uint m_u = uint(521288629);
uint m_v = uint(362436069);

uint GetUintCore(inout uint u, inout uint v)
{
	v = uint(36969) * (v & uint(65535)) + (v >> 16);
	u = uint(18000) * (u & uint(65535)) + (u >> 16);
	return (v << 16) + u;
}

float GetUniformCore(inout uint u, inout uint v)
{
	uint z = GetUintCore(u, v);

	return float(z) / uint(4294967295);
}

float rand()
{
	return GetUniformCore(m_u, m_v);
}

vec3 random_in_unit_sphere()
{
	vec3 p;

	float theta = rand() * 2.0 * PI;
	float phi   = rand() * PI;
	p.y = cos(phi);
	p.x = sin(phi) * cos(theta);
	p.z = sin(phi) * sin(theta);

	return p;
}
////////////////////////////////////////////////////

bool LambertianScatter(in Lambertian lambertian, in Ray incident, in HitRecord hitRecord, out Ray scattered, out vec3 attenuation)
{
	attenuation = lambertian.albedo;

	scattered.origin = hitRecord.position;
	scattered.direction = hitRecord.normal + random_in_unit_sphere();

	return true;
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
			rec.materialType = sphere.materialType;
			rec.material = sphere.material;

			return true;
		}

		temp = (-b + sqrt(discriminant)) / (2.0 * a);
		if(temp < t_max && temp > t_min)
		{
			rec.t = temp;
			rec.position = RayGetPointAt(ray, rec.t);
			rec.normal = normalize(rec.position - sphere.center);
			rec.materialType = sphere.materialType;
			rec.material = sphere.material;

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

bool MaterialScatter(in int materialType, in int material, in Ray incident, in HitRecord hitRecord, out Ray scatter, out vec3 attenuation)
{
	if(materialType == MAT_LAMBERTIAN)
	{
		return LambertianScatter(lambertMaterials[material] , incident, hitRecord, scatter, attenuation);
	}
}

//	光线追踪
vec3 WorldTrace(Ray ray, World world, int depth)
{
	HitRecord hitRecord;

	vec3 frac = vec3(1.0);
	vec3 bgColor = vec3(0.0);

	while(depth > 0)
	{
		depth--;
		if(WorldHit(world, ray, 0.1, 100000.0, hitRecord))
		{
			Ray scaterRay;
			vec3 attenuation;
			if(!MaterialScatter(hitRecord.materialType, hitRecord.material, ray, hitRecord, scaterRay, attenuation))
			{
				break;
			}

			frac *= attenuation;
			ray = scaterRay;
		}
		else
		{
			//	背景颜色
			vec3 dir = normalize(ray.direction);
			float t = (dir.y + 1.0) / 2.0;
			bgColor =  mix(vec3(1.0), vec3(0.5, 0.7, 1.0), t);
			break;
		}
	}

	return bgColor * frac;
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

void main()
{
	float u = fragPos.x;
	float v = fragPos.y;

	lambertMaterials[0] = LambertianConstructor(vec3(0.7, 0.5, 0.5));
	lambertMaterials[1] = LambertianConstructor(vec3(0.5, 0.7, 0.5));
	lambertMaterials[2] = LambertianConstructor(vec3(0.5, 0.5, 0.7));
	lambertMaterials[3] = LambertianConstructor(vec3(0.7, 0.7, 0.7));

	World world = NewWorld();

	vec3 col = vec3(0.0);

	vec2 texSize = 1.0 / vec2(800, 600);
	for(int i=-1; i<=1; i++)
	{
		for(int j=-1; j<=1; j++)
		{
			Ray ray = AARay(camera, texCoord + vec2(i, j) * texSize * 0.5);
			col += WorldTrace(ray, world, 50);
		}
	}
	col /= 9.0;
	col = pow(col, vec3(1.0 / 2.0));

	fragColor = vec4(col, 1.0);
}