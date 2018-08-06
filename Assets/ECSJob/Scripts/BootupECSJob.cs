using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEditor;

namespace AudioVizECSJob
{

public class BootupECSJob
{
    public static MeshInstanceRenderer cubeLook;
    public static Settings settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

        GameObject proto = GameObject.Find("CubePrototype");
        if(proto != null)
        {
            cubeLook = proto.GetComponent<MeshInstanceRendererComponent>().Value;
            Object.Destroy(proto);

            GameObject go = GameObject.Find("Settings");
            settings = go.GetComponent<Settings>();

            int channelCount = World.Active.GetOrCreateManager<SpectrumSystem>().Init(settings);
            World.Active.GetOrCreateManager<SpawnCubeSystem>().Init(settings, 
                                                                    channelCount,
                                                                    entityManager,
                                                                    cubeLook);

            World.Active.GetExistingManager<TransformSystem>().Enabled = false;
        }
    }
}

}
