using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MaterialData : IComponentData
{
    public float3 Color;
}
