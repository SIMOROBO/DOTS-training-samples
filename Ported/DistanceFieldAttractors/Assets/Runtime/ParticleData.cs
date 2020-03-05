using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ParticleData : IComponentData
{
    public float3 Position;
    public float3 Velocity;
}
