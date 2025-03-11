using UnityEngine;
using UnityEngine.UI;

public class AlphaController : MonoBehaviour
{
    //public Material targetMaterial; // Assign the material in Inspector
    public GameObject targetObject;
    public Slider alphaSlider; // Assign the UI Slider in Inspector

    private void Start()
    {
        if (alphaSlider && targetObject)
        {

            // Set initial slider value based on material's alpha
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                alphaSlider.value = targetRenderer.material.color.a; //targetMaterial.color.a;
                alphaSlider.onValueChanged.AddListener(UpdateAlpha); // Listen for slider changes
            }
            else
            {
                Debug.LogError("No renderer with a material found on selected gameObject");
            }
        }
    }

    void UpdateAlpha(float value)
    {
        //Debug.Log($"targetMaterial: {targetMaterial.name}");
        try
        {
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                Debug.LogWarning("No renderers found on object");
            foreach (Renderer renderer in renderers)
            {
                renderer.material = new Material(renderer.sharedMaterial);
                renderer.material.ConvertToTransparent();
                renderer.material.SetAlpha(value);
            }
            
        }
        catch {
            Debug.Log("target object is null: AlphaController");
        }
    }

    
    /*
    public Renderer FindRendererWithMaterial(GameObject obj)
    {
        // Check if this object has a Renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            return renderer;
        }

        // Otherwise, check children
        foreach (Transform child in obj.transform)
        {
            Renderer childRenderer = FindRendererWithMaterial(child.gameObject);
            if (childRenderer != null)
            {
                return childRenderer;
            }
        }

        return null;
    }*/

}



public static class ExtensionMethods
{
    public static void ConvertToTransparent(this Material material)
    {
        if (material == null) return;
        // Set the material to Transparent mode
        material.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        material.SetFloat("_Mode", 3); // 3 = Transparent in Standard Shader
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0); // Disable depth writing for transparency
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000; // Renders after opaque objects

        Debug.Log($"Material {material.name} converted to Transparent mode.");
    }
    public static void SetAlpha(this Material material, float value)
    {
        Color color = material.color;
        color.a = Mathf.Clamp(value, 0, 1);
        material.color = color;
        //material.SetColor("_Color", color);
        //material.SetColor("_BaseColor", color);
    }

}