using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class ObjectSelector : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown dropdown;

    [SerializeField]
    GameObject[] gameObjects; // Array of textures (assign in Inspector)

    public AlphaController alphaController;

    private void Start()
    {
        if (dropdown)
        {
            PopulateDropdown();
            dropdown.onValueChanged.AddListener(UpdateActiveObject);
        }
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (GameObject obj in gameObjects)
        {
            options.Add(obj.name);
        }

        dropdown.AddOptions(options);
    }

    void UpdateActiveObject(int active)
    {
        if (active >= 0 && active < gameObjects.Length)
        {
            for(int i = 0; i < gameObjects.Length; i++) 
            {
                gameObjects[i].SetActive(i == active); // only init if active index of dropdown
            }
            alphaController.targetObject = gameObjects[active];
            alphaController.targetRenderer = alphaController.FindRendererWithMaterial(alphaController.targetObject);
        }
    }
}
