using UnityEngine;
using Unity.Entities;

public enum DistanceFieldModels
{
    SpherePlane,
    Metaballs,
    SpinMixer,
    SphereField,
    FigureEight,
    PerlinNoise
}

[GenerateAuthoringComponent]
public struct DistanceFieldModeData : IComponentData
{
    public DistanceFieldModels Model;
    public double ElapsedTime;
}
