using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

namespace AudioVizECSJob
{

[System.Serializable]
public struct Scale : IComponentData
{
    public float3 Value;
}
 
[UnityEngine.ExecuteInEditMode]
public class CustomTransformSystem : JobComponentSystem
{
    struct CustomTransformGroup
    {
        public int Length;

        [ReadOnly]
        public ComponentDataArray<Position> Positions;
 
        [ReadOnly]
        public ComponentDataArray<LocalRotation> Rotations;
 
        [ReadOnly]
        public ComponentDataArray<Scale> Scales;
 
        public ComponentDataArray<TransformMatrix> Transforms;
    }
 
    [Inject]
    CustomTransformGroup transformGroup;
 
    [ComputeJobOptimization]
    struct CustomTransformGroupJob : IJobParallelFor
    {
        [ReadOnly]
        public ComponentDataArray<Position> Positions;
 
        [ReadOnly]
        public ComponentDataArray<LocalRotation> Rotations;
 
        [ReadOnly]
        public ComponentDataArray<Scale> Scales;
 
        public ComponentDataArray<TransformMatrix> Transforms;
 
        public void Execute(int i)
        {
            Transforms[i] = new TransformMatrix
            {
                Value = math.mul(math.rottrans(Quaternion.Euler(Rotations[i].Value.value.xyz), Positions[i].Value), math.scale(Scales[i].Value))
            };
        }
    }
 
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var transformJob = new CustomTransformGroupJob
        {
            Transforms = transformGroup.Transforms,
            Positions = transformGroup.Positions,
            Rotations = transformGroup.Rotations,
            Scales = transformGroup.Scales,
        };
 
        return transformJob.Schedule(transformGroup.Length, 64, inputDeps);
    }
}

}
