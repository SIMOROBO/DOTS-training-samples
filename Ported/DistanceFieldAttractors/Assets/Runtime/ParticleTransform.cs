using Unity.Mathematics;
using Unity.Entities;


[GenerateAuthoringComponent]
public struct ParticleTransform : IComponentData
{
    public float4x4 transform;
}
