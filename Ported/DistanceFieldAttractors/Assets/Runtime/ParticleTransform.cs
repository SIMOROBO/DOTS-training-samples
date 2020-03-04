using System.Numerics;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct ParticleTransform : IComponentData
{
    public Matrix4x4 transform;
}
