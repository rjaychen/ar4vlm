using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEditor;
/*
public class IMUData : MonoBehaviour
{

    private List<string> imuData = new List<string>();
    private string filePath;
    private bool isRecording = true;
    private StringBuilder sb = new StringBuilder("Time,AccelX,AccelY,AccelZ,GyroX,GyroY,GyroZ,MagX,MagY,MagZ");

    private void Awake()
    {
        var cur_time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
        var folder = Application.persistentDataPath;
#endif
        filePath = Path.Combine(Application.persistentDataPath, $"../files/imu_data_{cur_time}.csv");
    }

#if !UNITY_EDITOR
    private void OnEnable()
    {
        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        InputSystem.EnableDevice(Accelerometer.current);
        InputSystem.EnableDevice(MagneticFieldSensor.current);
    }

    private void OnDisable()
    {
        InputSystem.DisableDevice(UnityEngine.InputSystem.Gyroscope.current);
        InputSystem.DisableDevice(Accelerometer.current);
        InputSystem.DisableDevice(MagneticFieldSensor.current);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRecording) return;
        var acceleration = Accelerometer.current.acceleration.ReadValue();
        var angularVelocity = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
        var magneticField = MagneticFieldSensor.current.magneticField.ReadValue();
        
        float time = Time.time;
        sb.Append('\n').Append(time.ToString()).Append(",")
        .Append(acceleration.x.ToString()).Append(",").Append(acceleration.y.ToString()).Append(",").Append(acceleration.z.ToString()).Append(",")
        .Append(angularVelocity.x.ToString()).Append(",").Append(angularVelocity.y.ToString()).Append(",").Append(angularVelocity.z.ToString()).Append(",")
        .Append(magneticField.x.ToString()).Append(",").Append(magneticField.y.ToString()).Append(",").Append(magneticField.z.ToString());
    }

#endif

    private void OnApplicationQuit()
    {
        SaveCSV();
    }

    private void SaveCSV()
    {
        var content = sb.ToString();
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        Debug.Log($"IMU data saved to: {filePath}");
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
*/