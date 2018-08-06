using Unity.Entities;
using Unity.Mathematics;

namespace AudioVizECSJob
{

public struct Cube : IComponentData {}

public struct Origin : IComponentData 
{
    public float3 Value;
}

public struct LeftSpectrum : IComponentData {}
public struct RightSpectrum : IComponentData {}

}
