using UnityEngine;
using Unity.Collections;

public struct SpiralPos
{
    public float x;
    public float z;
}

public class Utils 
{
    public static SpiralPos GetSpiralPos(int n)
    {
        int x = 0, z = 0;
        if(--n >= 0) 
        {
            int v = (int)Mathf.Floor(Mathf.Sqrt(n + 0.25f) - 0.5f);
            int spiralBaseIndex = v * (v + 1);
            int flipFlop = ((v & 1) << 1) - 1;
            int offset = flipFlop * ((v + 1) >> 1);
            x += offset; z += offset;

            int cornerIndex = spiralBaseIndex + (v + 1);
            if(n < cornerIndex) 
            {
                x -= flipFlop * (n - spiralBaseIndex + 1);
            } 
            else 
            {
                x -= flipFlop * (v + 1);
                z -= flipFlop * (n - cornerIndex + 1);
            }
        }

        SpiralPos result = new SpiralPos();
        result.x = x;
        result.z = z;

        return result;
    }
}
