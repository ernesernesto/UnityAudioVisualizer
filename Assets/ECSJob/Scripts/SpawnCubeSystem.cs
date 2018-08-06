using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Rendering;

namespace AudioVizECSJob
{

public class SpawnCubeSystem : ComponentSystem 
{
    bool isFirstTime;
    int totalCubeCount;

    Settings settings;

    public void Init(Settings inSettings, int channelCount, EntityManager entityManager, MeshInstanceRenderer look)
    {

        isFirstTime = true;
        settings = inSettings;

        totalCubeCount = settings.spectrumSize*2;
        for(int index = 0; 
            index < totalCubeCount; 
            ++index)
        {
            bool left = index < settings.spectrumSize ? true : false;
            EntityArchetype archetype = entityManager.CreateArchetype(typeof(Cube), 
                                                                      typeof(Origin),
                                                                      typeof(TransformMatrix), 
                                                                      typeof(Position), 
                                                                      typeof(LocalRotation),
                                                                      typeof(Scale),
                                                                      left ? typeof(LeftSpectrum) :
                                                                             typeof(RightSpectrum));

            Entity cube = entityManager.CreateEntity(archetype);
            entityManager.AddSharedComponentData(cube, look); 
        }
    }

    public struct CubesLeftGroup
    {
        public int Length;
        public ComponentDataArray<Cube> cubes;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Origin> origins;
        public ComponentDataArray<LeftSpectrum> spectrums;
    }

    public struct CubesRightGroup
    {
        public int Length;
        public ComponentDataArray<Cube> cubes;
        public ComponentDataArray<Position> positions;
        public ComponentDataArray<Origin> origins;
        public ComponentDataArray<RightSpectrum> spectrums;
    }

    [Inject] 
    CubesLeftGroup leftGroup;

    [Inject] 
    CubesRightGroup rightGroup;

    protected override void OnUpdate()
    {
        if(isFirstTime)
        {
            int spectrumSize = settings.spectrumSize;
            for(int index = 0;
                index < totalCubeCount;
                ++index)
            {
                int xOffset;
                int groupIndex = index;
                if(index < spectrumSize)
                {
                    xOffset = -35;

                    SpiralPos pos = Utils.GetSpiralPos(groupIndex);
                    Position position = leftGroup.positions[groupIndex];
                    position.Value = new float3(pos.x + xOffset, 0, pos.z);
                    leftGroup.positions[groupIndex] = position;

                    Origin origin = leftGroup.origins[groupIndex];
                    origin.Value = position.Value;
                    leftGroup.origins[groupIndex] = origin;
                }
                else
                {
                    xOffset = 35;
                    groupIndex -= spectrumSize;

                    SpiralPos pos = Utils.GetSpiralPos(groupIndex);
                    Position position = rightGroup.positions[groupIndex];
                    position.Value = new float3(pos.x + xOffset, 0, pos.z);
                    rightGroup.positions[groupIndex] = position;

                    Origin origin = rightGroup.origins[groupIndex];
                    origin.Value = position.Value;
                    rightGroup.origins[groupIndex] = origin;
                }
            }
        }

        isFirstTime = false;
    }
}

}
