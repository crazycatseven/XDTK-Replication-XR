using UnityEngine;
using System;

public class UIEventProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "UIEvent";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    [Serializable]
    public class UIEventData
    {
        public string eventType;    // 例如："ButtonClick", "SliderChange"
        public string elementId;    // UI元素的标识符
        public string value;        // 可选的额外数据

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static UIEventData FromJson(string json)
        {
            return JsonUtility.FromJson<UIEventData>(json);
        }
    }

    public void SendUIEvent(string eventType, string elementId, string value = "")
    {
        if (!IsEnabled) return;

        UIEventData eventData = new UIEventData
        {
            eventType = eventType,
            elementId = elementId,
            value = value
        };

        string jsonData = eventData.ToJson();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, data);
    }
}