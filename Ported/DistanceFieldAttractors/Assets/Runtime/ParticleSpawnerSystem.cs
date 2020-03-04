using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct ParticlSpawner : IComponentData
{
    public int Count;
    public int Radius;
    public Entity Prefab;
}

public class ParticleSpawnerSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, in ParticlSpawner spawner, in Translation translation) =>
        {
            var particles = EntityManager.Instantiate(spawner.Prefab, spawner.Count, Allocator.TempJob);
            var positions = new NativeArray<float3>(spawner.Count, Allocator.TempJob);
            GeneratePoints.RandomPointsInSphere(translation.Value, spawner.Radius, positions);
            for (var i = 0; i < spawner.Count; i++)
            {
                EntityManager.SetComponentData(particles[i], new ParticleData { Position = positions[i] });
                if (EntityManager.HasComponent<Translation>(particles[i]))
                {
                    EntityManager.SetComponentData(particles[i], new Translation { Value = positions[i] });
                }
            }
            positions.Dispose();
            particles.Dispose();
            EntityManager.DestroyEntity(entity);
        }).Run();
        return default;
    }
}