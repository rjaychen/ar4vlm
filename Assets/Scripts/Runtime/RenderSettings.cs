using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RenderSettingsData
{
    public string objectName;
    public float opacity;
    public Vector3 sessionOriginPos;
    public Vector3 cameraPos;
    public Quaternion cameraRot;
    public Vector3 objectPos;
    public Quaternion objectRot;

    public string resolution;
    public Quaternion virtualLightDirection;
    public Quaternion realLightDirection;
    public float shadowIntensity;
    public float brightness;
    // might have to add user toggles for certain params.
    // will be a hassle but this is doable.

    public void SaveRenderSettings(string objectName)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, $"../files/render_settings/");
        if (!Directory.Exists(Path.GetDirectoryName(directoryPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(directoryPath));
        }
        int newIndex = Directory.GetFiles(directoryPath, $"android_{objectName}_*_RenderSettings.json")
                .Select(f => Path.GetFileNameWithoutExtension(f).Split('_'))
                .Where(parts => parts.Length >= 4 && int.TryParse(parts[2], out _))
                .Select(parts => int.Parse(parts[2]))
                .DefaultIfEmpty(0)
                .Max() + 1;
        string filePath = Path.Combine(directoryPath, $"android_{objectName}_{newIndex}_RenderSettings.json");
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Render settings saved to: " + filePath);
    }
}
