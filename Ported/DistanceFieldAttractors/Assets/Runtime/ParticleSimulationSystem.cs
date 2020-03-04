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

[AlwaysUpdateSystem]
public class ParticleSimulationSystem : JobComponentSystem
{
    //private EntityQuery m_ParticleQuery;
    [BurstCompile]
    struct ParticleSimulationJob : IJobForEach<ParticleData, MaterialData, Translation>
    {
        public DistanceFieldModel Model;
        public float Time;
        public Random Rnd;

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

        // what's the shortest distance from a given point to the isosurface?
        float GetDistance(float x, float y, float z, out float3 normal)
        {

            float distance = float.MaxValue;
            normal = float3.zero;
            if (Model == DistanceFieldModel.Metaballs)
            {
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
            }
            else if (Model == DistanceFieldModel.SpinMixer)
            {
                for (int i = 0; i < 6; i++)
                {
                    float orbitRadius = (i / 2 + 2) * 2;
                    float angle = Time * 20f * (1f + i * 0.1f);
                    float cx = math.cos(angle) * orbitRadius;
                    float cy = math.sin(angle);
                    float cz = math.sin(angle) * orbitRadius;

                    float newDist = Sphere(x - cx, y - cy, z - cz, 2f);
                    if (newDist < distance)
                    {
                        normal = new float3(x - cx, y - cy, z - cz);
                        distance = newDist;
                    }
                }
            }
            else if (Model == DistanceFieldModel.SpherePlane)
            {
                float sphereDist = Sphere(x, y, z, 5f);
                float3 sphereNormal = math.normalize(new float3(x, y, z));

                float planeDist = y;
                float3 planeNormal = new float3(0f, 1f, 0f);

                float t = math.sin(Time * 8f) * 0.4f + 0.4f;
                distance = math.lerp(sphereDist, planeDist, t);
                normal = math.lerp(sphereNormal, planeNormal, t);
            }
            else if (Model == DistanceFieldModel.SphereField)
            {
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
            }
            else if (Model == DistanceFieldModel.FigureEight)
            {
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
            }
            else if (Model == DistanceFieldModel.PerlinNoise)
            {
                float perlin = noise.cnoise(new float2(x * 0.2f, z * 0.2f));
                distance = y - perlin * 6f;
                normal = math.up();
            }

            return distance;
        }

        public void Execute(ref ParticleData particleData, ref MaterialData materialData, ref Translation translation)
        {
            //var position = particleData.Position;
            var position = translation.Value;
            var velocity = particleData.Velocity;

            var attraction = 0.4f;
            var jitter = 0.1f;

            float dist = GetDistance(position.x, position.y, position.z, out float3 normal);
            velocity -= math.normalize(normal) * attraction * math.clamp(dist, -1f, 1f);
            velocity += Rnd.NextFloat3Direction() * jitter;
            velocity *= 0.99f;
            velocity = new float3(dist, 0, 0);
            position += velocity;

            particleData.Position = position;
            translation.Value = position;
            particleData.Velocity = velocity;
        }

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //var particleCount = m_ParticleQuery.CalculateEntityCount();
        var particleSimulationJob = new ParticleSimulationJob
        {
            Model = DistanceFieldModel.Metaballs,
            Time = Time.DeltaTime,
            Rnd = new Random(123),
        };
        
        var particleHandle = particleSimulationJob.Schedule(this);
        return particleHandle;
        //return inputDeps;
    }

    //protected override void OnCreate()
    //{
    //    m_ParticleQuery = GetEntityQuery(new EntityQueryDesc
    //    {
    //        All = new[] { ComponentType.ReadWrite<ParticleData>(), ComponentType.ReadWrite<MaterialData>(), ComponentType.ReadWrite<Translation>() }
    //    });
    //}
}
