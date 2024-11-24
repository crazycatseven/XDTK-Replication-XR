using UnityEngine;
using System;

public class UIEventProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "UIEvent";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    // 定义所有支持的UI事件类型
    public static class EventTypes
    {
        // 基础UI事件
        public const string ButtonClick = "ButtonClick";
        public const string SliderChange = "SliderChange";
        public const string ToggleChange = "ToggleChange";
        public const string InputFieldChange = "InputFieldChange";

        // 超级按钮事件
        public const string SuperButtonClick = "SuperButtonClick";
        public const string SuperButtonHold = "SuperButtonHold";
        public const string SuperButtonDrag = "SuperButtonDrag";
        public const string SuperButtonRelease = "SuperButtonRelease";
    }

    [Serializable]
    public class UIEventData
    {
        [SerializeField]
        private string eventType;    // 改为private，通过方法设置
        [SerializeField]
        private string elementId;    // 改为private，通过方法设置
        [SerializeField]
        private string value;        // 改为private，通过方法设置

        // 提供公共属性来访问
        public string EventType => eventType;
        public string ElementId => elementId;
        public string Value => value;

        // 构造函数
        public UIEventData(string eventType, string elementId, string value = "")
        {
            ValidateEventType(eventType);
            this.eventType = eventType;
            this.elementId = elementId;
            this.value = value;
        }

        private void ValidateEventType(string eventType)
        {
            // 验证事件类型是否合法
            var field = typeof(EventTypes).GetField(eventType);
            if (field == null)
            {
                Debug.LogError($"Invalid UI event type: {eventType}");
                throw new ArgumentException($"Invalid UI event type: {eventType}");
            }
        }

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

        try
        {
            UIEventData eventData = new UIEventData(eventType, elementId, value);
            string jsonData = eventData.ToJson();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
            OnDataSend?.Invoke(DataType, data);
        }
        catch (ArgumentException e)
        {
            Debug.LogError($"Failed to send UI event: {e.Message}");
        }
    }
}