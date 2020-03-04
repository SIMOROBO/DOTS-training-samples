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

[AlwaysUpdateSystem]
public class ParticleSimulationSystem : JobComponentSystem
{
    //private EntityQuery m_ParticleQuery;
    [BurstCompile]
    struct ParticleSimulationJob : IJobForEach<ParticleData, MaterialData, Translation>
    {
        float GetDistance(float3 position, out float3 normal)
        {
            normal = 0;

            return 0.02f;
        }

        public void Execute(ref ParticleData particleData, ref MaterialData materialData, ref Translation translation)
        {
            //var position = particleData.Position;
            var position = translation.Value;
            var velocity = particleData.Velocity;

            var attraction = 0.4f;
            var jitter = 0.1f;

            float dist = GetDistance(position, out float3 normal);
            //velocity -= (float3)Vector3.Normalize(normal) * attraction * Mathf.Clamp(dist, -1f, 1f);
            //velocity += (float3)UnityEngine.Random.insideUnitSphere * jitter;
            //velocity *= 0.99f;
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
        var particleSimulationJob = new ParticleSimulationJob();
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
