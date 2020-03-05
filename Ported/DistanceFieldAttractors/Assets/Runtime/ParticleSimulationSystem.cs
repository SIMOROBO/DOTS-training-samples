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

    // Smooth-Minimum, from Media Molecule's "Dreams"
    [BurstCompile]
    static float SmoothMin(float a, float b, float radius)
    {
        var e = math.max(radius - math.abs(a - b), 0f);
        return math.min(a, b) - e * e * 0.25f / radius;
    }

    [BurstCompile]
    static float Sphere(float3 position, float radius)
    {
        return math.length(position) - radius;
    }

    [BurstCompile]
    struct GetDistanceMetaballsJob : IJobForEachWithEntity<ParticleData>
    {
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var position = particleData.Position;
            var distance = float.MaxValue;
            var normal = float3.zero;

            for (var i = 0; i < 5; i++)
            {
                var orbitRadius = i * 0.5f + 2f;
                var time = Time * 4f;
                var angle1 = time * (1f + i * 0.1f);
                var angle2 = time * (1.2f + i * 0.117f);
                var angle3 = time * (1.3f + i * 0.1618f);
                var c = new float3(math.cos(angle1), math.sin(angle2), math.sin(angle3)) * orbitRadius;

                var diff = position - c;
                var newDist = SmoothMin(distance, Sphere(diff, 2f), 2f);
                if (newDist < distance)
                {
                    normal = diff;
                    distance = newDist;
                }
            }

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct GetDistanceSpinMixerJob : IJobForEachWithEntity<ParticleData>
    {
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var position = particleData.Position;
            var distance = float.MaxValue;
            var normal = float3.zero;

            for (var i = 0; i < 6; i++)
            {
                var orbitRadius = (i / 2 + 2) * 2;
                var anglea = Time * 20f * (1f + i * 0.1f);
                var sinAngle = math.sin(anglea);
                var c = new float3(math.cos(anglea) * orbitRadius, sinAngle, sinAngle * orbitRadius);

                var diff = position - c;
                var newDist = Sphere(diff, 2f);

                if (newDist < distance)
                {
                    normal = diff;
                    distance = newDist;
                }
            }

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct GetDistanceSpherePlaneJob : IJobForEachWithEntity<ParticleData>
    {
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var position = particleData.Position;

            var sphereDist = Sphere(position, 5f);
            var sphereNormal = math.normalize(position);

            var planeDist = position.y;
            var planeNormal = new float3(0f, 1f, 0f);

            var t = math.sin(Time * 8f) * 0.4f + 0.4f;
            var distance = math.lerp(sphereDist, planeDist, t);
            var normal = math.lerp(sphereNormal, planeNormal, t);

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct GetDistanceSphereFieldJob : IJobForEachWithEntity<ParticleData>
    {
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var position = particleData.Position;

            float spacing = 5f + math.sin(Time * 5f) * 2f;
            var spacingVector = new float3(1f, 1f, 1f) * spacing * 0.5f;
            position += spacingVector;
            position -= math.floor(position / spacing) * spacing;
            position -= spacingVector;
            var distance = Sphere(position, 5f);
            var normal = position;

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct GetDistanceFigureEightJob : IJobForEachWithEntity<ParticleData>
    {
        public float Time;
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var x = particleData.Position.x;
            var y = particleData.Position.y;
            var z = particleData.Position.z;

            var ringRadius = 4f;
            var flipper = 1f;
            if (z < 0f)
            {
                z = -z;
                flipper = -1f;
            }
            var point = math.normalize(new float3(x, 0f, z - ringRadius)) * ringRadius;
            var angle = math.atan2(point.z, point.x) + Time * 8f;
            point += new float3(0f, 0f, 1f) * ringRadius;
            var normal = new float3(x - point.x, y - point.y, (z - point.z) * flipper);
            var wave = math.cos(angle * flipper * 3f) * 0.5f + 0.5f;
            wave *= wave * 0.5f;
            var distance = math.length(normal) - (0.5f + wave);

            Distances[index] = distance;
            Normals[index] = normal;
        }
    }

    [BurstCompile]
    struct GetDistancePerlinNoiseJob : IJobForEachWithEntity<ParticleData>
    {
        public NativeArray<float> Distances;
        public NativeArray<float3> Normals;

        public void Execute(Entity entity, int index, ref ParticleData particleData)
        {
            var x = particleData.Position.x;
            var y = particleData.Position.y;
            var z = particleData.Position.z;

            var  perlin = noise.cnoise(new float2(x * 0.2f, z * 0.2f));
            var distance = y - perlin * 6f;
            var normal = math.up();

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
    struct ParticleMaterialSimulationJob : IJobForEachWithEntity<MaterialData>
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

        // Get Distance
        var getDistanceHandle = inputDeps;
        switch (distanceFieldData.Model)
        {
            case DistanceFieldModels.Metaballs:
                var getDistanceMetaballsJob = new GetDistanceMetaballsJob
                {
                    Time = (float)distanceFieldData.ElapsedTime,
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistanceMetaballsJob.Schedule(this, inputDeps);
                break;
            case DistanceFieldModels.SpinMixer:
                var getDistanceSpinMixerJob = new GetDistanceSpinMixerJob
                {
                    Time = (float)distanceFieldData.ElapsedTime,
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistanceSpinMixerJob.Schedule(this, inputDeps);
                break;
            case DistanceFieldModels.SpherePlane:
                var getDistanceSpherePlaneJob = new GetDistanceSpherePlaneJob
                {
                    Time = (float)distanceFieldData.ElapsedTime,
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistanceSpherePlaneJob.Schedule(this, inputDeps);
                break;
            case DistanceFieldModels.SphereField:
                var getDistanceSphereFieldJob = new GetDistanceSphereFieldJob
                {
                    Time = (float)distanceFieldData.ElapsedTime,
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistanceSphereFieldJob.Schedule(this, inputDeps);
                break;
            case DistanceFieldModels.FigureEight:
                var getDistanceFigureEightJob = new GetDistanceFigureEightJob
                {
                    Time = (float)distanceFieldData.ElapsedTime,
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistanceFigureEightJob.Schedule(this, inputDeps);
                break;
            case DistanceFieldModels.PerlinNoise:
                var getDistancePerlineNoiseJob = new GetDistancePerlinNoiseJob
                {
                    Distances = distances,
                    Normals = normals,
                };
                getDistanceHandle = getDistancePerlineNoiseJob.Schedule(this, inputDeps);
                break;
        }

        // Position
        var particlePositionSimulationJob = new ParticlePositionSimulationJob
        {
            Rnd = new Random(123),
            Attraction = particleManagerData.Attraction,
            Jitter = particleManagerData.Jitter,
            Distances = distances,
            Normals = normals,
        };
        var particlePositionHandle = particlePositionSimulationJob.Schedule(this, getDistanceHandle);

        // Material
        var particleMaterialSimulationJob = new ParticleMaterialSimulationJob
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
