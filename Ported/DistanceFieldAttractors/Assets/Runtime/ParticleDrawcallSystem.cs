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

    protected override void OnCreate()
    {
        m_query = GetEntityQuery(typeof(ParticleTransform));
    }

    protected override void OnUpdate()
    {
        var transforms = m_query.ToComponentDataArray<ParticleTransform>(Unity.Collections.Allocator.TempJob);
        var t = transforms.Reinterpret<Matrix4x4>();
        
        var shader = Shader.Find("Instanced/InstancedShader");
        if (shader != null)
        {
            Material material = new Material(shader);
            Mesh mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube);
            
            if (material != null && mesh != null && t != null)
            {
                if(m_particleDrawcall != null)
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
        }
        t.Dispose();

    }
}
