using Unity.Entities;
using UnityEngine;

public class FixedTimeUpdater : MonoBehaviour
{
    ParticleSimulationSystem particleSimulationSystem;

    void FixedUpdate()
    {
        if (particleSimulationSystem == null)
        {
            particleSimulationSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ParticleSimulationSystem>();
        }
        
        particleSimulationSystem.Update();
    }
}
