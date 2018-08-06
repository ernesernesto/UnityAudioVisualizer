using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using UnityEngine.Jobs;

namespace AudioVizMonobehavior
{

public class BootupMono : MonoBehaviour
{
    public GameObject cubePrefab;
    public Settings settings;

    float[] spectrumBuff;
    float[] prevSpectrumBuff;

    GameObject[] cubes;

    void Start()
    {
        int spectrumSize = settings.spectrumSize;
        spectrumBuff = new float[spectrumSize];
        prevSpectrumBuff = new float[spectrumSize];
        cubes = new GameObject[spectrumSize];

        for(int index = 0;
            index < spectrumSize;
            ++index)
        {
            GameObject obj = Object.Instantiate(cubePrefab);

            SpiralPos pos = Utils.GetSpiralPos(index);
            obj.transform.position = new Vector3(pos.x, 0, pos.z);
            obj.transform.parent = this.transform;
            cubes[index] = obj;
        }
    }

    void Update()
    {
        AudioListener.GetSpectrumData(spectrumBuff, 0, FFTWindow.BlackmanHarris);

        float dynamics = settings.dynamics;
        float maxScale = settings.maxScale;
        float rotationSpeed = settings.rotationSpeed;
        float epsilon = settings.epsilon;
        int spectrumLength = spectrumBuff.Length;

        for(int index = 0; 
            index < spectrumLength; 
            index++)
        {
            float val = (dynamics*prevSpectrumBuff[index] + (1 - dynamics)*spectrumBuff[index]);
            prevSpectrumBuff[index] = val;

            if(val >= epsilon)
            {
                cubes[index].transform.localScale = new Vector3(1, val*maxScale, 1);
                float halfHeight = cubes[index].transform.localScale.y*0.5f;
                Vector3 origin = cubes[index].transform.position;
                cubes[index].transform.position = new Vector3(origin.x, halfHeight, origin.z);
            }
            else
            {
                cubes[index].transform.localScale = new Vector3(0, 0, 0);
            }
        }

        gameObject.transform.Rotate(0, Time.deltaTime*rotationSpeed, 0);
    }
}

}
