using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

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
public struct DistanceFieldData : IComponentData
{
    public DistanceFieldModels Model;
    public double ElapsedTime;
    public KeyCode ChangeModeKey;
}

[AlwaysSynchronizeSystem]
public class DistanceFieldSelectionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltatime = Time.DeltaTime;
        
        Entities.WithoutBurst().ForEach((ref DistanceFieldData distanceField) => {

            var mode = distanceField.Model;
            var currentMode = (int)mode;
            var time = distanceField.ElapsedTime + deltatime;

            if (time >= 10 || Input.GetKey(distanceField.ChangeModeKey))
            {
                currentMode += 1;
                
                if(currentMode == 5)
                {
                    currentMode = 0;
                }

                distanceField.Model = (DistanceFieldModels)currentMode;
                time = 0;
            }

            distanceField.ElapsedTime = time;
        }).Run();

        return default;
    }
}
