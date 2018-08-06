using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;

namespace AudioVizECSJob
{

public class Channel
{
    public int channelNumber;
    public float[] spectrumBuff;
    public Transform[] transforms;
    public NativeArray<float> currentSpectrums;
    public NativeArray<float> prevSpectrums;
    public NativeArray<Vector3> origins;

    public void Init(int channel, int spectrumSize)
    {
        channelNumber = channel;
        spectrumBuff = new float[spectrumSize];
        transforms = new Transform[spectrumSize];
        currentSpectrums = new NativeArray<float>(spectrumSize, Allocator.Persistent);
        prevSpectrums = new NativeArray<float>(spectrumSize, Allocator.Persistent);
        origins = new NativeArray<Vector3>(spectrumSize, Allocator.Persistent);
    }
}

class SpectrumSystem : JobComponentSystem
{
    Settings settings;

    Channel[] channels;

    public int Init(Settings inSettings)
    {
        settings = inSettings;

        int spectrumSize = settings.spectrumSize;
        channels = new Channel[2];
        channels[0] = new Channel();
        channels[0].Init(0, spectrumSize);
        channels[1] = new Channel();
        channels[1].Init(1, spectrumSize);

        int result = channels.Length;
        return result;
    }
    
    struct LeftSpectrumGroup
    {
        public int Length;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Origin> origins;
        public ComponentDataArray<Scale> scales;
        public ComponentDataArray<LeftSpectrum> spectrums;
    }

    struct RightSpectrumGroup
    {
        public int Length;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Origin> origins;
        public ComponentDataArray<Scale> scales;
        public ComponentDataArray<RightSpectrum> spectrums;
    }

    [Inject] 
    LeftSpectrumGroup leftGroup;

    [Inject] 
    RightSpectrumGroup rightGroup;

    [ComputeJobOptimization]
    public struct SpectrumJob : IJobParallelFor
    {
        public float maxScale;
        public float dynamics;
        public float epsilon;

        public NativeArray<float> currentSpectrums;
        public NativeArray<float> prevSpectrums;

        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Origin> origins;
        public ComponentDataArray<Scale> scales;

        public void Execute(int index)
        {
            float current = currentSpectrums[index];
            float prev = prevSpectrums[index];

            float val = (dynamics*prev + (1 - dynamics)*current);
            prevSpectrums[index] = val;

            float valAdjusted = val*maxScale;
            float halfHeight = valAdjusted*0.5f;

            Position position = positions[index];
            Origin origin = origins[index];
            Scale scale = scales[index];

            if(val >= epsilon)
            {
                position.Value = new float3(origin.Value.x, halfHeight, origin.Value.z); 
                scale.Value = new float3(1, valAdjusted, 1);
            }
            else
            {
                position.Value = new float3(0, 0, 0); 
                scale.Value = new float3(0, 0, 0);
            }

            scales[index] = scale;
            positions[index] = position;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle leftHandle = CreateJob(true, channels[0], inputDeps);
        JobHandle rightHandle = CreateJob(false, channels[1], leftHandle);
        return rightHandle;
    }

    JobHandle CreateJob(bool left, Channel channel, JobHandle inputDeps)
    {
        AudioListener.GetSpectrumData(channel.spectrumBuff, 
                                      channel.channelNumber, 
                                      FFTWindow.BlackmanHarris);
        channel.currentSpectrums.CopyFrom(channel.spectrumBuff);

        ComponentDataArray<Position> inPositions;
        ComponentDataArray<Origin> inOrigins;
        ComponentDataArray<Scale> inScales;
        int groupLength;

        if(left)
        {
            inPositions = leftGroup.positions;
            inOrigins = leftGroup.origins;
            inScales = leftGroup.scales;
            groupLength = leftGroup.Length;
        }
        else
        {
            inPositions = rightGroup.positions;
            inOrigins = rightGroup.origins;
            inScales = rightGroup.scales;
            groupLength = rightGroup.Length;
        }

        SpectrumJob spectrumJob = new SpectrumJob()
        {
            maxScale = settings.maxScale,
            dynamics = settings.dynamics,
            epsilon = settings.epsilon,
            currentSpectrums = channel.currentSpectrums,
            prevSpectrums = channel.prevSpectrums,

            positions = inPositions,
            origins = inOrigins,
            scales = inScales
        };

        JobHandle handle = spectrumJob.Schedule(groupLength, 1, inputDeps);
        return handle;
    }
}

}
