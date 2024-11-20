using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

public class SensorDataHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "SensorData" };

    [System.Serializable]
    public class SensorDataEvent : UnityEvent<SensorDataProvider.SensorData> { }

    public SensorDataEvent OnSensorDataReceived;

    private void Awake()
    {
        if (OnSensorDataReceived == null)
            OnSensorDataReceived = new SensorDataEvent();
    }

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType == "SensorData")
        {
            string jsonData = Encoding.UTF8.GetString(data);
            var sensorData = SensorDataProvider.SensorData.FromJson(jsonData);
            OnSensorDataReceived.Invoke(sensorData);
        }
        else
        {
            Debug.LogWarning($"SensorDataHandler: Unsupported data type {dataType} received.");
        }
    }
}