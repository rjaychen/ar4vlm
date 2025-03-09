using UnityEngine;
using TMPro;

public class TextureSelector : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown dropdown;

    [SerializeField]
    Material targetMaterial;

    [SerializeField]
    Texture[] textures; // Array of textures (assign in Inspector)

    private void Start()
    {
        if (dropdown)
        {
            dropdown.onValueChanged.AddListener(ChangeTexture);
        }
    }

    void ChangeTexture(int index)
    {
        if (index >= 0 && index < textures.Length)
        {
            targetMaterial.mainTexture = textures[index];
        }
    }
}
