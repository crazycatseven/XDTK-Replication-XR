using UnityEngine;
using System.Text;
using System;

public class ObjectUpdateProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "ObjectUpdate";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    public void SendObjectUpdate(ObjectUpdate update)
    {
        if (!IsEnabled) return;
        
        string jsonData = JsonUtility.ToJson(update);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, data);
    }

    [Serializable]
    public class ObjectUpdate
    {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ObjectUpdate FromJson(string json)
        {
            return JsonUtility.FromJson<ObjectUpdate>(json);
        }
    }
}
