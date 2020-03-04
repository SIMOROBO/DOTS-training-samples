﻿
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class ParticleTransformSystem : JobComponentSystem
{
    EntityQuery m_query;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_query = GetEntityQuery(typeof(ParticleManagerData));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var particleManagerDatas = m_query.ToComponentDataArray<ParticleManagerData>(Unity.Collections.Allocator.TempJob);
        var handleDeps = inputDeps;

        if (particleManagerDatas != null && particleManagerDatas.Length > 0)
        {
            var particleManagerData = particleManagerDatas[0];
            var upVector = new float3(0, 1, 0);

            handleDeps = Entities.ForEach((Entity entity, ref ParticleTransform particleTransform, in ParticleData particleData, in MaterialData materialData) =>
            {
                var scale = new float3(0.1f, 0.01f, math.max(0.1f, math.length(particleData.Velocity) * particleManagerData.SpeedStretch));
                var q = quaternion.LookRotation(math.normalize(particleData.Velocity), upVector);

                float4x4 matrix = float4x4.TRS(particleData.Position, q, scale);
                matrix.c0.w = materialData.Color.x;
                matrix.c1.w = materialData.Color.y;
                matrix.c2.w = materialData.Color.z;
                particleTransform.transform = matrix;

            }).Schedule(inputDeps);
        }

        particleManagerDatas.Dispose();
        return handleDeps;
    }
}
