using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ParticleDrawcallSystem))]
public class ParticleTransformSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // 1 - Initialization
        // 2 - Sim group
        // 3 - Presentation group

        Entities.ForEach((Entity entity, ref ParticleTransform particleTransform, in ParticleData particleData, in MaterialData materialData) =>
        {
            var m = System.Numerics.Matrix4x4.CreateTranslation(new System.Numerics.Vector3(particleData.Position.x, particleData.Position.y, particleData.Position.z));
            particleTransform.transform = m;
            
        }).Run();
        return default;
    }
}
