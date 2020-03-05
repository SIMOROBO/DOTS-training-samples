using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using System;


public class DistanceFieldSelectionSystem : JobComponentSystem
{
    private int _ModesCount;

    protected override void OnCreate()
    {
        _ModesCount = Enum.GetNames(typeof(DistanceFieldModels)).Length;
    }

    [BurstCompile]
    struct DistanceFieldSelectionJob : IJobForEach<DistanceFieldModeData>
    {

        public float deltaTime;
        public int modesCount;
        public bool keyUp;

        public void Execute(ref DistanceFieldModeData ModeData)
        {
            var mode = ModeData.Model;
            var currentMode = (int)mode;
            var time = ModeData.ElapsedTime + deltaTime;

            if (time >= 10 || keyUp)
            {
                currentMode += 1;

                if (currentMode == modesCount)
                {
                    currentMode = 0;
                }

                ModeData.Model = (DistanceFieldModels)currentMode;
                time = 0;
            }

            ModeData.ElapsedTime = time;
        }
            
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ModeSelectionJob = new DistanceFieldSelectionJob
        {
            deltaTime = Time.DeltaTime,
            modesCount = _ModesCount,
            keyUp = Input.GetKeyUp(KeyCode.Space)
        };

        var ModeHandle = ModeSelectionJob.Schedule(this);
       
        return ModeHandle;
    }
}
