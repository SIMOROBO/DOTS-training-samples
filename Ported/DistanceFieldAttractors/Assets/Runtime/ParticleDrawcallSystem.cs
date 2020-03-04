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
                var _drawcall = DrawCallGenerator.GetDrawCall(mesh, material, t);
                Graphics.DrawMeshInstancedIndirect(_drawcall.Mesh, _drawcall.SubMeshIndex, _drawcall.Material, _drawcall.Bounds, _drawcall.ArgsBuffer);
            }
        }
        t.Dispose();

    }
}
