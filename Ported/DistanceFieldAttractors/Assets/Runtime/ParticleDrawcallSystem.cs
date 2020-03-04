using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class ParticleDrawcallSystem : ComponentSystem
{
    EntityQuery m_query;
    ParticleDrawCall m_particleDrawcall;
    List<ParticleManagerSharedData> m_particleData = new List<ParticleManagerSharedData>(2);

    protected override void OnCreate()
    {
        m_query = GetEntityQuery(typeof(ParticleTransform));
    }

    protected override void OnUpdate()
    {

        var transforms = m_query.ToComponentDataArray<ParticleTransform>(Unity.Collections.Allocator.TempJob);
        var t = transforms.Reinterpret<Matrix4x4>();

        EntityManager.GetAllUniqueSharedComponentData(m_particleData);
        if (m_particleData.Count == 0)
        {
            return;
        }

        var material = m_particleData[1].ParticleMaterial;
        var mesh = m_particleData[1].ParticleMesh;

        if (material != null && mesh != null && t != null)
        {
            if (m_particleDrawcall != null)
            {
                m_particleDrawcall.Release();
            }

            m_particleDrawcall = DrawCallGenerator.GetDrawCall(mesh, material, t);

            Graphics.DrawMeshInstancedIndirect(
                m_particleDrawcall.Mesh,
                m_particleDrawcall.SubMeshIndex,
                m_particleDrawcall.Material,
                m_particleDrawcall.Bounds,
                m_particleDrawcall.ArgsBuffer);
        }

        transforms.Dispose();
    }

    protected override void OnDestroy()
    {
        if (m_particleDrawcall != null)
        {
            m_particleDrawcall.Release();
        }
    }
}
