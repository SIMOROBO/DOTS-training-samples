using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ParticleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<ParticleData>(entity);
        dstManager.AddComponent<MaterialData>(entity);
        dstManager.AddComponent<ParticleTransform>(entity);

        // Remove components that we don't need
        if (dstManager.HasComponent<Translation>(entity))
            dstManager.RemoveComponent<Translation>(entity);
        if (dstManager.HasComponent<Rotation>(entity))
            dstManager.RemoveComponent<Rotation>(entity);
        if (dstManager.HasComponent<LocalToWorld>(entity))
            dstManager.RemoveComponent<LocalToWorld>(entity);
    }
}
