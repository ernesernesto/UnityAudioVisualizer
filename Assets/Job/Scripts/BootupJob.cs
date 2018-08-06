using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using UnityEngine.Jobs;

namespace AudioVizJob
{

public class BootupJob : MonoBehaviour
{
    public GameObject cubePrefab;
    public Settings settings;

    public Transform parentLeft;
    public Transform parentRight;

    SpectrumWrapper left;
    SpectrumWrapper right;

    void Start()
    {
        int spectrumSize = settings.spectrumSize;

        left = InitChannel(0, spectrumSize, parentLeft);
        right = InitChannel(1, spectrumSize, parentRight);
    }

    SpectrumWrapper InitChannel(int channel, int spectrumSize, Transform parent)
    {
        SpectrumWrapper result = new SpectrumWrapper();
        result.channel = channel;
        result.spectrumBuff = new float[spectrumSize];
        result.transforms = new Transform[spectrumSize];
        result.currentSpectrums = new NativeArray<float>(spectrumSize, Allocator.Persistent);
        result.prevSpectrums = new NativeArray<float>(spectrumSize, Allocator.Persistent);
        result.origins = new NativeArray<Vector3>(spectrumSize, Allocator.Persistent);

        for(int index = 0;
            index < spectrumSize;
            ++index)
        {
            GameObject obj = Object.Instantiate(cubePrefab);

            SpiralPos pos = Utils.GetSpiralPos(index);

            obj.transform.parent = parent;
            obj.transform.localPosition = new Vector3(pos.x, 0, pos.z);

            result.transforms[index] = obj.transform;
            result.origins[index] = obj.transform.localPosition;
        }

        result.transformsAccess = new TransformAccessArray(result.transforms);

        return result;
    }

    void Update()
    {
        float dynamics = settings.dynamics;
        float maxScale = settings.maxScale;
        float rotationSpeed = settings.rotationSpeed;
        float epsilon = settings.epsilon;

        gameObject.transform.Rotate(Vector3.up*(rotationSpeed*Time.deltaTime));

        left.CreateJob(maxScale, dynamics, epsilon);
        right.CreateJob(maxScale, dynamics, epsilon);
    }
 
    void LateUpdate()
    {
        left.CompleteJob();
        right.CompleteJob();
    }

    void OnDestroy()
    {
        left.Destroy();
        right.Destroy();
    }
}

}

struct SpectrumJob : IJobParallelForTransform
{
    public float maxScale;
    public float dynamics;
    public float epsilon;
    public NativeArray<float> currentSpectrums;
    public NativeArray<float> prevSpectrums;
    public NativeArray<Vector3> origins;

    public void Execute(int index, TransformAccess transform)
    {
        float current = currentSpectrums[index];
        float prev = prevSpectrums[index];

        float val = (dynamics*prev + (1 - dynamics)*current);
        prevSpectrums[index] = val;

        float valAdjusted = val*maxScale;
        float halfHeight = valAdjusted*0.5f;

        Vector3 origin = origins[index];
        if(val >= epsilon)
        {
            transform.localPosition = new Vector3(origin.x, halfHeight, origin.z);
            transform.localScale = new Vector3(1, valAdjusted, 1);
        }
        else
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
    }
}

public class SpectrumWrapper
{
    public int channel;
    public float[] spectrumBuff;
    public Transform[] transforms;
    public NativeArray<float> currentSpectrums;
    public NativeArray<float> prevSpectrums;
    public NativeArray<Vector3> origins;
    public TransformAccessArray transformsAccess;

    SpectrumJob spectrumJob;
    JobHandle jobHandle;

    public void CreateJob(float inMaxScale, float inDynamics, float inEpsilon)
    {
        AudioListener.GetSpectrumData(spectrumBuff, channel, FFTWindow.BlackmanHarris);
        currentSpectrums.CopyFrom(spectrumBuff);

        spectrumJob = new SpectrumJob()
        {
            maxScale = inMaxScale,
            dynamics = inDynamics,
            epsilon = inEpsilon,
            currentSpectrums = this.currentSpectrums,
            prevSpectrums = this.prevSpectrums,
            origins = this.origins,
        };

        jobHandle = spectrumJob.Schedule(transformsAccess);
    }

    public void CompleteJob()
    {
        jobHandle.Complete();
        spectrumJob.prevSpectrums.CopyTo(prevSpectrums);
    }

    public void Destroy()
    {
        currentSpectrums.Dispose();
        prevSpectrums.Dispose();
        origins.Dispose();
        transformsAccess.Dispose();
    }
}
