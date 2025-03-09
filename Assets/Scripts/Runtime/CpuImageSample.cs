using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using System.IO;
using Unity.XR.CoreUtils;
using System.Linq;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// <para>This component tests getting the latest camera image and converting it to RGBA format. If successful,
    /// it displays the image on the screen as a Raw Image and also displays information about the image.
    /// This is useful for computer vision applications where you need to access the raw pixels from camera image
    /// on the CPU.</para>
    /// <para>This is different from the ARCameraBackground component, which efficiently displays the camera image on the screen.
    /// If you just want to blit the camera texture to the screen, use the ARCameraBackground, or use Graphics.Blit to create
    /// a GPU-friendly RenderTexture.</para>
    /// <para>In this example, we get the camera image data on the CPU, convert it to an RGBA format, then display it on the screen
    /// as a RawImage texture to demonstrate it is working. This is done as an example; do not use this technique simply
    /// to render the camera image on screen.</para>
    /// </summary>
    public class CpuImageSample : MonoBehaviour
    {
        Texture2D m_CameraTexture;
        XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorY;

        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        ARCameraManager m_CameraManager;

        [SerializeField]
        [Tooltip("The Camera which will produce AR-inclusive frames")]
        public Camera arCaptureCamera;

        [SerializeField]
        [Tooltip("The Camera which will produce raw image frames")]
        public Camera rawCaptureCamera;

        [SerializeField]
        [Tooltip("The main Directional Light for HDR Lighting Estimation")]
        public Light m_Light;

        [SerializeField]
        [Tooltip("Parent Model in Hierarchy of models to display")]
        public GameObject modelsParent;

        [SerializeField]
        public GameObject xrOrigin;

        [SerializeField]
        public AlphaController alphaController;

        public bool useLightingEstimation = true;

        public Vector3? mainLightDirection { get; private set; }
        public RenderTexture renderTexture;


        [SerializeField]
        [Tooltip("The AROcclusionManager which will produce human depth and stencil textures.")]
        AROcclusionManager m_OcclusionManager;

        [SerializeField]
        RawImage m_RawCameraImage;

        /// <summary>
        /// Get or set the UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawCameraImage
        {
            get => m_RawCameraImage;
            set => m_RawCameraImage = value;
        }

        [SerializeField]
        RawImage m_RawHumanDepthImage;

        /// <summary>
        /// The UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawHumanDepthImage
        {
            get => m_RawHumanDepthImage;
            set => m_RawHumanDepthImage = value;
        }

        [SerializeField]
        RawImage m_RawHumanStencilImage;

        /// <summary>
        /// The UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawHumanStencilImage
        {
            get => m_RawHumanStencilImage;
            set => m_RawHumanStencilImage = value;
        }

        [SerializeField]
        RawImage m_RawEnvironmentDepthImage;

        /// <summary>
        /// The UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawEnvironmentDepthImage
        {
            get => m_RawEnvironmentDepthImage;
            set => m_RawEnvironmentDepthImage = value;
        }

        [SerializeField]
        RawImage m_RawEnvironmentDepthConfidenceImage;

        /// <summary>
        /// The UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawEnvironmentDepthConfidenceImage
        {
            get => m_RawEnvironmentDepthConfidenceImage;
            set => m_RawEnvironmentDepthConfidenceImage = value;
        }

        [SerializeField]
        Text m_ImageInfo;

        /// <summary>
        /// The UI Text used to display information about the image on screen.
        /// </summary>
        public Text imageInfo
        {
            get => m_ImageInfo;
            set => m_ImageInfo = value;
        }

        [HideInInspector]
        [SerializeField]
        Button m_TransformationButton;

        /// <summary>
        /// The button that controls transformation selection.
        /// </summary>
        public Button transformationButton
        {
            get => m_TransformationButton;
            set => m_TransformationButton = value;
        }

        delegate bool TryAcquireDepthImageDelegate(out XRCpuImage image);

        /// <summary>
        /// Cycles the image transformation to the next case.
        /// </summary>
        public void CycleTransformation()
        {
            m_Transformation = m_Transformation switch
            {
                XRCpuImage.Transformation.None => XRCpuImage.Transformation.MirrorX,
                XRCpuImage.Transformation.MirrorX => XRCpuImage.Transformation.MirrorY,
                XRCpuImage.Transformation.MirrorY => XRCpuImage.Transformation.MirrorX | XRCpuImage.Transformation.MirrorY,
                _ => XRCpuImage.Transformation.None
            };

            if (m_TransformationButton)
            {
                m_TransformationButton.GetComponentInChildren<Text>().text = m_Transformation.ToString();
            }
        }

        public void SetLightingToggle(bool value) { useLightingEstimation = value; }

        private GameObject GetActiveObject()
        {
            foreach(Transform child in modelsParent.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        unsafe public void CaptureImage()
        {
            //string cur_time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            
            Vector3 originPos = xrOrigin.transform.position;
            Vector3 cameraPos = arCaptureCamera.transform.localPosition;
            Quaternion cameraRot = arCaptureCamera.transform.rotation;
            GameObject activeObject = GetActiveObject();

            CaptureRawImage(activeObject.name);
            CaptureARImage(activeObject.name);
            SaveDepthTexture(activeObject.name);

            // Save Render Settings into JSON
            RenderSettingsData settings = new RenderSettingsData
            {
                objectName = activeObject.name,
                sessionOriginPos = originPos,
                cameraPos = cameraPos,
                cameraRot = cameraRot,
                opacity = alphaController.alphaSlider.value,
                resolution = m_CameraManager.currentConfiguration.ToString(),
                virtualLightDirection = m_Light.transform.rotation,
                realLightDirection = Quaternion.LookRotation(mainLightDirection.Value),
                shadowIntensity = m_Light.shadowStrength,
                brightness = m_Light.intensity,
            };
            settings.SaveRenderSettings(activeObject.name);

            //TODO: save transparency, scale, camera pose, 
            /*
            try
            {
                var acceleration = Accelerometer.current.acceleration.ReadValue();
                var angularVelocity = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
                var magneticField = MagneticFieldSensor.current.magneticField.ReadValue();
            }
            catch { }
            */
        }

        private void CaptureRawImage(string objectName)
        {
            rawCaptureCamera.targetTexture = renderTexture;
            rawCaptureCamera.Render();

            Texture2D screenTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            screenTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenTexture.Apply();

            // Save the image
            //string path = Path.Combine(Application.persistentDataPath, $"../files/raw_images/{objectName}_raw.png");
            string directoryPath = Path.Combine(Application.persistentDataPath, "../files/raw_images/");
            if (!Directory.Exists(Path.GetDirectoryName(directoryPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(directoryPath));
            }
            // Get all matching files and find the highest index
            int newIndex = Directory.GetFiles(directoryPath, $"{objectName}_*_raw.png")
                .Select(f => Path.GetFileNameWithoutExtension(f).Split('_'))
                .Where(parts => parts.Length >= 3 && int.TryParse(parts[1], out _))
                .Select(parts => int.Parse(parts[1]))
                .DefaultIfEmpty(0)
                .Max() + 1;
            string filePath = Path.Combine(directoryPath, $"{objectName}_{newIndex}_raw.png");

            File.WriteAllBytes(filePath, screenTexture.EncodeToPNG());
            Debug.Log($"Raw scene image saved at: {filePath}");

            arCaptureCamera.targetTexture = null;
            RenderTexture.active = null;
        }

        private void CaptureARImage(string objectName)
        {
            arCaptureCamera.targetTexture = renderTexture;
            arCaptureCamera.Render();

            Texture2D screenTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            screenTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenTexture.Apply();

            // Save the image
            //string path = Path.Combine(Application.persistentDataPath, $"../files/ar_images/ar_image_{cur_time}.png");
            string directoryPath = Path.Combine(Application.persistentDataPath, "../files/ar_images/");
            if (!Directory.Exists(Path.GetDirectoryName(directoryPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(directoryPath));
            }
            // Get all matching files and find the highest index
            int newIndex = Directory.GetFiles(directoryPath, $"{objectName}_*_ar.png")
                .Select(f => Path.GetFileNameWithoutExtension(f).Split('_'))
                .Where(parts => parts.Length >= 3 && int.TryParse(parts[1], out _))
                .Select(parts => int.Parse(parts[1]))
                .DefaultIfEmpty(0)
                .Max() + 1;
            string filePath = Path.Combine(directoryPath, $"{objectName}_{newIndex}_ar.png");

            File.WriteAllBytes(filePath, screenTexture.EncodeToPNG());
            Debug.Log($"AR scene image saved at: {filePath}");

            arCaptureCamera.targetTexture = null;
            RenderTexture.active = null;

            // save render settings here;

        }

        private void _CaptureRawImage(string cur_time)
        {
            if (!m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                return;

            var conversionParams = new XRCpuImage.ConversionParams(image, TextureFormat.RGBA32, m_Transformation);
            var texture = new Texture2D(image.width, image.height);
            var rawTextureData = texture.GetRawTextureData<byte>();
            try
            {
                unsafe
                {
                    image.Convert(
                        conversionParams,
                        new IntPtr(rawTextureData.GetUnsafePtr()),
                        rawTextureData.Length
                    );
                }
            }
            finally
            {
                image.Dispose();
            }
            texture.Apply();
            byte[] bytes = ImageConversion.EncodeToPNG(texture);
            string filePath = Path.Combine(Application.persistentDataPath, $"../files/raw_image_{cur_time}.png");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            try
            {
                File.WriteAllBytes(filePath, bytes);
                Debug.Log("Saved data to: " + filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to save data to: " + filePath);
                Debug.LogWarning(e);
            }
        }

        void SaveDepthTexture(string cur_time)
        {
            if (m_OcclusionManager == null || m_OcclusionManager.environmentDepthTexture == null)
            {
                Debug.LogError("No environment depth texture available to save.");
                return;
            }

            // Get the depth texture
            Texture2D depthTexture = m_OcclusionManager.environmentDepthTexture;

            // Create a new Texture2D with same dimensions and format
            RenderTexture renderTexture = RenderTexture.GetTemporary(depthTexture.width, depthTexture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(depthTexture, renderTexture);

            Texture2D readableTexture = new Texture2D(depthTexture.width, depthTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            readableTexture.ReadPixels(new Rect(0, 0, depthTexture.width, depthTexture.height), 0, 0);
            readableTexture.Apply();

            // Convert to PNG
            byte[] pngData = readableTexture.EncodeToPNG();
            if (pngData != null)
            {
                string filePath = Path.Combine(Application.persistentDataPath, $"../files/depth_textures/depth_texture_{cur_time}.png");
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                System.IO.File.WriteAllBytes(filePath, pngData);
                Debug.Log($"Depth texture saved at: {filePath}");
            }

            // Cleanup
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            Destroy(readableTexture);
        }


        /* //don't use this function 
        unsafe void CaptureDepthImage(string cur_time)
        {
            if (!m_OcclusionManager.TryAcquireRawEnvironmentDepthCpuImage(out XRCpuImage image))
                return;

            var conversionParams = new XRCpuImage.ConversionParams(image, TextureFormat.RGBA32, m_Transformation);
            var texture = new Texture2D(image.width, image.height);
            var rawTextureData = texture.GetRawTextureData<byte>();
            try
            {
                unsafe
                {
                    image.Convert(
                        conversionParams,
                        new IntPtr(rawTextureData.GetUnsafePtr()),
                        rawTextureData.Length
                    );
                }
            }
            finally
            {
                image.Dispose();
            }
            texture.Apply();
            byte[] bytes = ImageConversion.EncodeToPNG(texture);
            string filePath = Path.Combine(Application.persistentDataPath, $"../files/depth_image_{cur_time}.png");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            try
            {
                File.WriteAllBytes(filePath, bytes);
                Debug.Log("Saved data to: " + filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to save data to: " + filePath);
                Debug.LogWarning(e);
            }
        }
        */
        void OnEnable()
        {
            if (m_CameraManager == null)
            {
                Debug.LogException(new NullReferenceException(
                    $"Serialized properties were not initialized on {name}'s {nameof(CpuImageSample)} component."), this);
                return;
            }

            m_CameraManager.frameReceived += OnCameraFrameReceived;

            // Dynamically adjust rendertexture settings
            var config = m_CameraManager.currentConfiguration;
            if (config.HasValue)
            {
                int width = config.Value.width;
                int height = config.Value.height;
                renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
            }
            else
            {
                renderTexture = new RenderTexture(1080, 1920, 16, RenderTextureFormat.ARGB32); // Default fallback
            }
        }

        void OnDisable()
        {
            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= OnCameraFrameReceived;
        }

        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            UpdateCameraImage();
            // add a toggle that sets light direction to estimated light or uses default light...
            if (eventArgs.lightEstimation.mainLightDirection.HasValue)
            {
                mainLightDirection = eventArgs.lightEstimation.mainLightDirection;
                if (useLightingEstimation)
                {
                    m_Light.transform.rotation = Quaternion.LookRotation(mainLightDirection.Value);
                }
            }

            //UpdateDepthImage(m_OcclusionManager.TryAcquireHumanDepthCpuImage, m_RawHumanDepthImage);
            //UpdateDepthImage(m_OcclusionManager.TryAcquireHumanStencilCpuImage, m_RawHumanStencilImage);
            //UpdateDepthImage(m_OcclusionManager.TryAcquireEnvironmentDepthCpuImage, m_RawEnvironmentDepthImage);
            //UpdateDepthImage(m_OcclusionManager.TryAcquireEnvironmentDepthConfidenceCpuImage, m_RawEnvironmentDepthConfidenceImage);
        }

        unsafe void UpdateCameraImage()
        {
            // Attempt to get the latest camera image. If this method succeeds,
            // it acquires a native resource that must be disposed (see below).
            if (!m_CameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                return;
            }

            // Display some information about the camera image
            m_ImageInfo.text = string.Format(
                "Image info:\n\twidth: {0}\n\theight: {1}\n\tplaneCount: {2}\n\ttimestamp: {3}\n\tformat: {4}",
                image.width, image.height, image.planeCount, image.timestamp, image.format);

            // Once we have a valid XRCpuImage, we can access the individual image "planes"
            // (the separate channels in the image). XRCpuImage.GetPlane provides
            // low-overhead access to this data. This could then be passed to a
            // computer vision algorithm. Here, we will convert the camera image
            // to an RGBA texture and draw it on the screen.

            // Choose an RGBA format.
            // See XRCpuImage.FormatSupported for a complete list of supported formats.
            const TextureFormat format = TextureFormat.RGBA32;

            if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
                m_CameraTexture = new Texture2D(image.width, image.height, format, false);

            // Convert the image to format, flipping the image across the Y axis.
            // We can also get a sub rectangle, but we'll get the full image here.
            var conversionParams = new XRCpuImage.ConversionParams(image, format, m_Transformation);

            // Texture2D allows us write directly to the raw texture data
            // This allows us to do the conversion in-place without making any copies.
            var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
            try
            {
                image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
            }
            finally
            {
                // We must dispose of the XRCpuImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }

            // Apply the updated texture data to our texture
            m_CameraTexture.Apply();

            // Set the RawImage's texture so we can visualize it.
            m_RawCameraImage.texture = m_CameraTexture;
        }

        /// <summary>
        /// Calls <paramref name="tryAcquireDepthImageDelegate"/> and renders the resulting depth image contents to <paramref name="rawImage"/>.
        /// </summary>
        /// <param name="tryAcquireDepthImageDelegate">The method to call to acquire a depth image.</param>
        /// <param name="rawImage">The Raw Image to use to render the depth image to the screen.</param>
        void UpdateDepthImage(TryAcquireDepthImageDelegate tryAcquireDepthImageDelegate, RawImage rawImage)
        {
            if (tryAcquireDepthImageDelegate(out XRCpuImage cpuImage))
            {
                // XRCpuImages, if successfully acquired, must be disposed.
                // You can do this with a using statement as shown below, or by calling its Dispose() method directly.
                using (cpuImage)
                {
                    UpdateRawImage(rawImage, cpuImage, m_Transformation);
                }
            }
            else
            {
                rawImage.enabled = false;
            }
        }

        static void UpdateRawImage(RawImage rawImage, XRCpuImage cpuImage, XRCpuImage.Transformation transformation)
        {
            // Get the texture associated with the UI.RawImage that we wish to display on screen.
            var texture = rawImage.texture as Texture2D;

            // If the texture hasn't yet been created, or if its dimensions have changed, (re)create the texture.
            // Note: Although texture dimensions do not normally change frame-to-frame, they can change in response to
            //    a change in the camera resolution (for camera images) or changes to the quality of the human depth
            //    and human stencil buffers.
            if (texture == null || texture.width != cpuImage.width || texture.height != cpuImage.height)
            {
                texture = new Texture2D(cpuImage.width, cpuImage.height, cpuImage.format.AsTextureFormat(), false);
                rawImage.texture = texture;
            }

            // For display, we need to mirror about the vertical access.
            var conversionParams = new XRCpuImage.ConversionParams(cpuImage, cpuImage.format.AsTextureFormat(), transformation);

            // Get the Texture2D's underlying pixel buffer.
            var rawTextureData = texture.GetRawTextureData<byte>();

            // Make sure the destination buffer is large enough to hold the converted data (they should be the same size)
            Debug.Assert(rawTextureData.Length == cpuImage.GetConvertedDataSize(conversionParams.outputDimensions, conversionParams.outputFormat),
                "The Texture2D is not the same size as the converted data.");

            // Perform the conversion.
            cpuImage.Convert(conversionParams, rawTextureData);

            // "Apply" the new pixel data to the Texture2D.
            texture.Apply();

            // Make sure it's enabled.
            rawImage.enabled = true;
        }
    }
}
