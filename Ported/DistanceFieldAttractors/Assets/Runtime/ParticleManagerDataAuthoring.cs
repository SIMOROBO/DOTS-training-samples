using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ParticleManagerData : ISharedComponentData, IEquatable<ParticleManagerData>
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

    public bool Equals(ParticleManagerData other)
    {
        return
            Attraction == other.Attraction &&
            SpeedStretch == other.SpeedStretch &&
            Jitter == other.Jitter &&
            ReferenceEquals(ParticleMesh, other.ParticleMesh) &&
            ReferenceEquals(ParticleMaterial, other.ParticleMaterial) &&
            SurfaceColor == other.SurfaceColor &&
            InteriorColor == other.InteriorColor &&
            ExteriorColor == other.ExteriorColor &&
            ExteriorColorDist == other.ExteriorColorDist &&
            InteriorColorDist == other.InteriorColorDist &&
            ColorStiffness == other.ColorStiffness;
    }

    public override int GetHashCode()
    {
        int hash = Attraction.GetHashCode();

        //if (!ReferenceEquals(SpeedStretch, null))
            hash ^= SpeedStretch.GetHashCode();
        //if (!ReferenceEquals(Jitter, null))
            hash ^= Jitter.GetHashCode();
        if (!ReferenceEquals(ParticleMesh, null))
            hash ^= ParticleMesh.GetHashCode();
        if (!ReferenceEquals(ParticleMaterial, null))
            hash ^= ParticleMaterial.GetHashCode();
        //if (!ReferenceEquals(SurfaceColor, null))
            hash ^= SurfaceColor.GetHashCode();
        //if (!ReferenceEquals(InteriorColor, null))
            hash ^= InteriorColor.GetHashCode();
        //if (!ReferenceEquals(ExteriorColor, null))
            hash ^= ExteriorColor.GetHashCode();
        //if (!ReferenceEquals(ExteriorColorDist, null))
            hash ^= ExteriorColorDist.GetHashCode();
        //if (!ReferenceEquals(InteriorColorDist, null))
            hash ^= InteriorColorDist.GetHashCode();
        //if (!ReferenceEquals(ColorStiffness, null))
            hash ^= ExteriorColorDist.GetHashCode();

        return hash;
    }
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
