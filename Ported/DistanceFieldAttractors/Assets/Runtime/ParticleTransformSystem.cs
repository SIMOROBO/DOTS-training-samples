
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ParticleDrawcallSystem))]
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
        // 1 - Initialization
        // 2 - Sim group
        // 3 - Presentation group

        var particleManagerDatas = m_query.ToComponentDataArray<ParticleManagerData>(Unity.Collections.Allocator.TempJob);
        if (particleManagerDatas != null && particleManagerDatas.Length > 0)
        {
            var particleManagerData = particleManagerDatas[0];
            Entities.ForEach((Entity entity, ref ParticleTransform particleTransform, in ParticleData particleData, in MaterialData materialData) =>
            {
                float3 scale = new float3(0.1f, 0.01f, Mathf.Max(0.1f, math.length(particleData.Velocity) * particleManagerData.SpeedStretch));
                var q = quaternion.LookRotation(math.normalize( particleData.Velocity), new float3(0, 1, 0));
                //var q = quaternion.LookRotation(particleData.Velocity, new float3(0, 1, 0));
                
                float4x4 matrix = float4x4.TRS(particleData.Position,  q, scale);
                matrix.c0.w = materialData.Color.x;
                matrix.c1.w = materialData.Color.y;
                matrix.c2.w = materialData.Color.z;
                particleTransform.transform = matrix;

            }).Run();
        }
        return default;
    }
}
