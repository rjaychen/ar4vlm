using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;

public class LightController : MonoBehaviour
{

    [SerializeField]
    CpuImageSample cpuImages;

    public void SetRotationY(float val)
    {
        if (!cpuImages.useLightingEstimation)
        {
            Quaternion rot = transform.rotation;
            Vector3 euler = rot.eulerAngles;
            euler.y = val;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
}
