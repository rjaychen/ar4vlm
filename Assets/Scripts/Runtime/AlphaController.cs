using UnityEngine;
using UnityEngine.UI;

public class AlphaController : MonoBehaviour
{
    //public Material targetMaterial; // Assign the material in Inspector
    public GameObject targetObject;
    public Renderer targetRenderer;
    private Material targetMaterial;
    public Slider alphaSlider; // Assign the UI Slider in Inspector

    private void Start()
    {
        if (alphaSlider && targetObject)
        {

            // Set initial slider value based on material's alpha
            targetRenderer = FindRendererWithMaterial(targetObject);
            print(targetRenderer.name);
            if (targetRenderer != null)
            {
                targetMaterial = targetRenderer.material;
                alphaSlider.value = targetMaterial.color.a; //targetMaterial.color.a;
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
        Debug.Log($"targetMaterial: {targetMaterial.name}");
        try
        {
            targetMaterial.SetAlpha(value);
        }
        catch {
            Debug.Log("target object is null: AlphaController");
        }
    }

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
    }

}



public static class ExtensionMethods
{

    public static void SetAlpha(this Material material, float value)
    {
        Color color = material.color;
        color.a = value;
        //material.SetColor("_Color", color);
        material.SetColor("_BaseColor", color);
    }

}