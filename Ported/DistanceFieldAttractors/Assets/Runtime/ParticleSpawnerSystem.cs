using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[GenerateAuthoringComponent]
public struct ParticlSpawner : IComponentData
{
    public int Count;
    public Entity Prefab;
}

public class ParticleSpawnerSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        Entities.WithStructuralChanges().WithoutBurst().ForEach((in ParticlSpawner spawner) =>
        {
            var particls = EntityManager.Instantiate(spawner.Prefab, spawner.Count, Allocator.Temp);
            var positions = new NativeArray<float3>(spawner.Count, Allocator.Temp);
            //GeneratePoints.RandomPointsInUnitSphere(positions);
            //positions.Dispose();
            particls.Dispose();
        }).Run();
        return default;
    }
}