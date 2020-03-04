﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ParticleManagerData : IComponentData
{
    public float Attraction;
    public float Jitter;
    public float3 SurfaceColor;
    public float3 InteriorColor;
    public float3 ExteriorColor;
    public float ExteriorColorDist;
    public float InteriorColorDist;
    public float ColorStiffness;
    public float SpeedStretch;
}

[Serializable]
public struct ParticleManagerSharedData : ISharedComponentData, IEquatable<ParticleManagerSharedData>
{
    
    public Mesh ParticleMesh;
    public Material ParticleMaterial;

    public bool Equals(ParticleManagerSharedData other)
    {
        return
            //SpeedStretch == other.SpeedStretch &&
            ReferenceEquals(ParticleMesh, other.ParticleMesh) &&
            ReferenceEquals(ParticleMaterial, other.ParticleMaterial);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        if (!ReferenceEquals(ParticleMesh, null))
            hash ^= ParticleMesh.GetHashCode();
        if (!ReferenceEquals(ParticleMaterial, null))
            hash ^= ParticleMaterial.GetHashCode();

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
        dstManager.AddComponentData(entity, new ParticleManagerData
        {
            Attraction = attraction,
            Jitter = jitter,
            SurfaceColor = new float3(surfaceColor.r, surfaceColor.g, surfaceColor.b),
            InteriorColor = new float3(interiorColor.r, interiorColor.g, interiorColor.b),
            ExteriorColor = new float3(exteriorColor.r, exteriorColor.g, exteriorColor.b),
            ExteriorColorDist = exteriorColorDist,
            InteriorColorDist = interiorColorDist,
            ColorStiffness = colorStiffness,
            SpeedStretch = speedStretch
        });
        dstManager.AddSharedComponentData(entity, new ParticleManagerSharedData
        {
            //SpeedStretch = speedStretch,
            ParticleMesh = particleMesh,
            ParticleMaterial = particleMaterial,
        });
        
    }
}
