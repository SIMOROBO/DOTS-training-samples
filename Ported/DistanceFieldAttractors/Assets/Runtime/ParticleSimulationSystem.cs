using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

//[AlwaysUpdateSystem]
[DisableAutoCreation]
public class ParticleSimulationSystem : JobComponentSystem
{
    private EntityQuery m_ParticleQuery;

    [BurstCompile]
    struct GetDistanceJob : IJobForEachWithEntity<ParticleData>
    {
        public DistanceFieldModels Model;
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        // Smooth-Minimum, from Media Molecule's "Dreams"
        float SmoothMin(float a, float b, float radius)
        {
            float e = math.max(radius - Mathf.Abs(a - b), 0);
            return math.min(a, b) - e * e * 0.25f / radius;
        }

        float Sphere(float x, float y, float z, float radius)
        {
            return math.sqrt(x * x + y * y + z * z) - radius;
        }

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {

            var x = particleData.Position.x;
            var y = particleData.Position.y;
            var z = particleData.Position.z;
            var distance = float.MaxValue;
            var normal = float3.zero;

            switch (Model)
            {
                case DistanceFieldModels.Metaballs:

                    for (int i = 0; i < 5; i++)
                    {
                        float orbitRadius = i * 0.5f + 2f;
                        float angle1 = Time * 4f * (1f + i * 0.1f);
                        float angle2 = Time * 4f * (1.2f + i * 0.117f);
                        float angle3 = Time * 4f * (1.3f + i * 0.1618f);
                        float cx = math.cos(angle1) * orbitRadius;
                        float cy = math.sin(angle2) * orbitRadius;
                        float cz = math.sin(angle3) * orbitRadius;

                        float newDist = SmoothMin(distance, Sphere(x - cx, y - cy, z - cz, 2f), 2f);
                        if (newDist < distance)
                        {
                            normal = new float3(x - cx, y - cy, z - cz);
                            distance = newDist;
                        }
                    }

                    break;

                case DistanceFieldModels.SpinMixer:
                    for (int i = 0; i < 6; i++)
                    {
                        float orbitRadius = (i / 2 + 2) * 2;
                        float anglea = Time * 20f * (1f + i * 0.1f);
                        float cx = math.cos(anglea) * orbitRadius;
                        float cy = math.sin(anglea);
                        float cz = math.sin(anglea) * orbitRadius;

                        float newDist = Sphere(x - cx, y - cy, z - cz, 2f);

                        if (newDist < distance)
                        {
                            normal = new float3(x - cx, y - cy, z - cz);
                            distance = newDist;
                        }
                    }

                    break;

                case DistanceFieldModels.SpherePlane:

                    float sphereDist = Sphere(x, y, z, 5f);
                    float3 sphereNormal = math.normalize(new float3(x, y, z));

                    float planeDist = y;
                    float3 planeNormal = new float3(0f, 1f, 0f);

                    float t = math.sin(Time * 8f) * 0.4f + 0.4f;
                    distance = math.lerp(sphereDist, planeDist, t);
                    normal = math.lerp(sphereNormal, planeNormal, t);

                    break;

                case DistanceFieldModels.SphereField:

                    float spacing = 5f + math.sin(Time * 5f) * 2f;
                    x += spacing * 0.5f;
                    y += spacing * 0.5f;
                    z += spacing * 0.5f;
                    x -= math.floor(x / spacing) * spacing;
                    y -= math.floor(y / spacing) * spacing;
                    z -= math.floor(z / spacing) * spacing;
                    x -= spacing * 0.5f;
                    y -= spacing * 0.5f;
                    z -= spacing * 0.5f;
                    distance = Sphere(x, y, z, 5f);
                    normal = new float3(x, y, z);

                    break;

                case DistanceFieldModels.FigureEight:

                    float ringRadius = 4f;
                    float flipper = 1f;
                    if (z < 0f)
                    {
                        z = -z;
                        flipper = -1f;
                    }
                    float3 point = math.normalize(new float3(x, 0f, z - ringRadius)) * ringRadius;
                    float angle = math.atan2(point.z, point.x) + Time * 8f;
                    point += new float3(0f, 0f, 1f) * ringRadius;
                    normal = new float3(x - point.x, y - point.y, (z - point.z) * flipper);
                    float wave = math.cos(angle * flipper * 3f) * 0.5f + 0.5f;
                    wave *= wave * 0.5f;
                    distance = math.sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z) - (0.5f + wave);

                    break;

                case DistanceFieldModels.PerlinNoise:

                    float perlin = noise.cnoise(new float2(x * 0.2f, z * 0.2f));
                    distance = y - perlin * 6f;
                    normal = math.up();

                    break;
            }

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct ParticlePositionSimulationJob : IJobForEachWithEntity<ParticleData>
    {
        public Random Rnd;
        public float Attraction;
        public float Jitter;
        [ReadOnly]
        public NativeArray<float> Distances;
        [ReadOnly]
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var position = particleData.Position;
            var velocity = particleData.Velocity;
            var dist = Distances[index];
            var normal = Normals[index];

            velocity -= math.normalize(normal) * Attraction * math.clamp(dist, -1f, 1f);
            velocity += Rnd.NextFloat3Direction() * Rnd.NextFloat() * Jitter;
            velocity *= 0.99f;
            position += velocity;

            particleData.Position = position;
            particleData.Velocity = velocity;
        }
    }

    [BurstCompile]
    struct ParticleMaterialSimulatoinJob : IJobForEachWithEntity<MaterialData>
    {
        public float DeltaTime;
        public float3 InteriorColor;
        public float3 ExteriorColor;
        public float3 SurfaceColor;
        public float InteriorColorDist;
        public float ExteriorColorDist;
        public float ColorStiffness;
        [ReadOnly]
        public NativeArray<float> Distances;

        public void Execute(Entity entity, int index, ref MaterialData materialData)
        {
            var dist = Distances[index];
            float3 targetColor;
            if (dist > 0f)
            {
                targetColor = math.lerp(SurfaceColor, ExteriorColor, dist / ExteriorColorDist);
            }
            else
            {
                targetColor = math.lerp(SurfaceColor, InteriorColor, -dist / InteriorColorDist);
            }

            materialData.Color = math.lerp(materialData.Color, targetColor, DeltaTime * ColorStiffness);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var distanceFieldData = GetSingleton<DistanceFieldModeData>();
        var particleManagerData = GetSingleton<ParticleManagerData>();

        var count = m_ParticleQuery.CalculateEntityCount();
        var distances = new NativeArray<float>(count, Allocator.TempJob);
        var normals = new NativeArray<float3>(count, Allocator.TempJob);

        var getDistanceJob = new GetDistanceJob
        {
            Model = distanceFieldData.Model,
            Time = (float)distanceFieldData.ElapsedTime,
            Distances = distances,
            Normals = normals,
        };
        var getDistanceHandle = getDistanceJob.Schedule(this, inputDeps);

        var particlePositionSimulationJob = new ParticlePositionSimulationJob
        {
            Rnd = new Random(123),
            Attraction = particleManagerData.Attraction,
            Jitter = particleManagerData.Jitter,
            Distances = distances,
            Normals = normals,
        };
        var particlePositionHandle = particlePositionSimulationJob.Schedule(this, getDistanceHandle);

        var particleMaterialSimulationJob = new ParticleMaterialSimulatoinJob
        {
            DeltaTime = Time.DeltaTime,
            InteriorColor = particleManagerData.InteriorColor,
            ExteriorColor = particleManagerData.ExteriorColor,
            SurfaceColor = particleManagerData.SurfaceColor,
            InteriorColorDist = particleManagerData.InteriorColorDist,
            ExteriorColorDist = particleManagerData.ExteriorColorDist,
            ColorStiffness = particleManagerData.ColorStiffness,
            Distances = distances,
        };
        var particleMaterialHandle = particleMaterialSimulationJob.Schedule(this, getDistanceHandle);

        var particleHandle = JobHandle.CombineDependencies(particlePositionHandle, particleMaterialHandle);

        //particleHandle.Complete();

        var disposeJobHandle = distances.Dispose(particleHandle);
        disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, normals.Dispose(particleHandle));
        particleHandle = disposeJobHandle;

        return particleHandle;
    }

    protected override void OnCreate()
    {
        m_ParticleQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadWrite<ParticleData>(), ComponentType.ReadWrite<MaterialData>() }
        });
    }
}
