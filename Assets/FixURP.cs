#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FixURP
{
    [MenuItem("Tools/Fix URP Global Settings")]
    static void Fix()
    {
        // UniversalRenderPipelineGlobalSettings is internal in the URP assembly.
        // Avoid a direct compile-time reference by creating the SO via its class name string
        // and casting to the public base type RenderPipelineGlobalSettings.
        var settings = ScriptableObject.CreateInstance("UniversalRenderPipelineGlobalSettings") as RenderPipelineGlobalSettings;
        if (settings == null)
        {
            Debug.LogError("Failed to create UniversalRenderPipelineGlobalSettings instance. Type may have changed in this URP version.");
            return;
        }

        AssetDatabase.CreateAsset(settings, "Assets/Settings/UniversalRenderPipelineGlobalSettings.asset");
        AssetDatabase.SaveAssets();

        GraphicsSettings.RegisterRenderPipelineSettings<UniversalRenderPipeline>(settings);
        Debug.Log("URP Global Settings created and registered!");
    }
}
#endif