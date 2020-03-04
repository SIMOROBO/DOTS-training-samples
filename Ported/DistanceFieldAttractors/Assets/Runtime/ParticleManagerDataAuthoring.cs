using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ParticleManagerData : ISharedComponentData
{
    public float Attraction;
    public float SpeedStretch;
    public float Jitter;
    public Mesh ParticleMesh;
    public Material ParticleMaterial;
    public Color SurfaceColor;
    public Color InteriorColor;
    public Color ExteriorColor;
    public float ExteriorColorDist;
    public float InteriorColorDist;
    public float ColorStiffness;
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ParticleManagerDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float attraction;
    public float speedStretch;
    public float jitter;
    public Mesh particleMesh;
    public Material particleMaterial;
    public Color surfaceColor;
    public Color interiorColor;
    public Color exteriorColor;
    public float exteriorColorDist = 3f;
    public float interiorColorDist = 3f;
    public float colorStiffness;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new ParticleManagerData
        {
            Attraction = attraction,
            SpeedStretch = speedStretch,
            Jitter = jitter,
            ParticleMesh = particleMesh,
            ParticleMaterial = particleMaterial,
            SurfaceColor = surfaceColor,
            InteriorColor = interiorColor,
            ExteriorColor = exteriorColor,
            ExteriorColorDist = exteriorColorDist,
            InteriorColorDist = interiorColorDist,
            ColorStiffness = colorStiffness,
        });
        
    }
}
