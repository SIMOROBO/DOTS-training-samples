
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[GenerateAuthoringComponent]
public struct SpawnerData : IComponentData
{
    public Entity ParticlePrefab;
    public int ParticleCount;
}

public class SpawnerSystem : JobComponentSystem
{

    private World _World = World.DefaultGameObjectInjectionWorld;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, in SpawnerData spawner ) =>
        {
            _World.EntityManager.Instantiate(spawner.ParticlePrefab, spawner.ParticleCount, Allocator.Temp);
            _World.EntityManager.DestroyEntity(entity);
        }).Run();

        return default;
    }
}
