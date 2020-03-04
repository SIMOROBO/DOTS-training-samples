using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MaterialData : IComponentData
{
    public float4 Color;
}
