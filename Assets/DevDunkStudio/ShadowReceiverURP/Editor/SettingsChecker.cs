using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DevDunk.ShadowReceiver.EditorTools
{
    public class URPChecker : Editor
    {
        [InitializeOnLoadMethod]
        static void CompilationStarted()
        {
            // Check URP settings for cascade count and soft shadows
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                if (urpAsset.supportsSoftShadows)
                {
                    Debug.LogWarning("Soft Shadows are enabled in URP settings. This is not supported by the free version of Shadow Receiver URP\n" +
                        "Disable it or consider upgrading to the paid version of Shadow Receiver URP: <a href=\"https://assetstore.unity.com/packages/vfx/shaders/shadow-receiver-urp-directional-additional-light-shadows-228069?aid=1100ljSxt\">Shadow Receiver URP https://assetstore.unity.com/packages/vfx/shaders/shadow-receiver-urp-directional-additional-light-shadows-228069?aid=1100ljSxt</a> ");
                }

                if (urpAsset.shadowCascadeCount > 1)
                {
                    Debug.LogWarning("Cascade Count is greater than 1 in URP settings. This is not supported by the free version of Shadow Receiver URP\n" +
                        "Reduce it to 1 or consider upgrading to the paid version of Shadow Receiver URP: <a href=\"https://assetstore.unity.com/packages/vfx/shaders/shadow-receiver-urp-directional-additional-light-shadows-228069?aid=1100ljSxt\">Shadow Receiver URP https://assetstore.unity.com/packages/vfx/shaders/shadow-receiver-urp-directional-additional-light-shadows-228069?aid=1100ljSxt</a> ");
                }
            }
            else
            {
                Debug.LogError("URP is not set as the current render pipeline. Shadow Receiver URP requires URP to work correctly. Please set URP as the current render pipeline in the Graphics Settings");
            }
        }
    }
}