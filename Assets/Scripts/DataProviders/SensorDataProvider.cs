using UnityEngine;
using System;

public class SensorDataProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "SensorData";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    [SerializeField]
    private float updateInterval = 0.1f; // 更新频率
    private float lastUpdateTime;

    [Serializable]
    public class SensorData
    {
        public Vector3 acceleration;      // 加速度
        public Vector3 gyroscope;         // 陀螺仪
        public Vector3 gravity;           // 重力
        public Quaternion deviceAttitude; // 设备朝向
        public float magneticHeading;     // 磁北方向
        public float trueHeading;         // 真北方向
        public float headingAccuracy;     // 方向精度

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static SensorData FromJson(string json)
        {
            return JsonUtility.FromJson<SensorData>(json);
        }
    }

    private void Update()
    {
        if (!IsEnabled || Time.time - lastUpdateTime < updateInterval)
            return;

        SendSensorData();
        lastUpdateTime = Time.time;
    }

    private void SendSensorData()
    {
        SensorData sensorData = new SensorData
        {
            acceleration = Input.acceleration,
            gyroscope = Input.gyro.rotationRate,
            gravity = Input.gyro.gravity,
            deviceAttitude = Input.gyro.attitude,
            magneticHeading = Input.compass.magneticHeading,
            trueHeading = Input.compass.trueHeading,
            headingAccuracy = Input.compass.headingAccuracy
        };

        string jsonData = sensorData.ToJson();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, data);
    }

    private void OnEnable()
    {
        Input.gyro.enabled = true;
        Input.compass.enabled = true;
    }

    private void OnDisable()
    {
        Input.gyro.enabled = false;
        Input.compass.enabled = false;
    }
}