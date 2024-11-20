using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIEventHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "UIEvent" };

    [System.Serializable]
    public class UIEventData
    {
        public string elementId;
        public string value;
    }

    [System.Serializable]
    public class UIEventMapping
    {
        public string elementId;
        public UnityEvent<string> onEventReceived;
    }

    [SerializeField]
    private List<UIEventMapping> eventMappings = new List<UIEventMapping>();
    private Dictionary<string, UnityEvent<string>> eventCache;

    private void Awake()
    {
        InitializeEventCache();
    }

    private void InitializeEventCache()
    {
        eventCache = new Dictionary<string, UnityEvent<string>>();
        foreach (var mapping in eventMappings)
        {
            if (!string.IsNullOrEmpty(mapping.elementId))
            {
                eventCache[mapping.elementId] = mapping.onEventReceived;
            }
        }
    }

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "UIEvent") return;

        string jsonData = System.Text.Encoding.UTF8.GetString(data);
        var eventData = UIEventProvider.UIEventData.FromJson(jsonData);
        Debug.Log($"Received UI Event - ID: {eventData.elementId}, Value: {eventData.value}");

        if (eventCache.TryGetValue(eventData.elementId, out var eventCallback))
        {
            eventCallback.Invoke(eventData.value);
        }
        else
        {
            Debug.LogWarning($"No handler found for UI element ID: {eventData.elementId}");
        }
    }

    public void AddEventHandler(string elementId, UnityAction<string> handler)
    {
        if (!eventCache.ContainsKey(elementId))
        {
            var newEvent = new UnityEvent<string>();
            eventCache[elementId] = newEvent;
            
            eventMappings.Add(new UIEventMapping 
            { 
                elementId = elementId,
                onEventReceived = newEvent
            });
        }
        
        eventCache[elementId].AddListener(handler);
    }
}