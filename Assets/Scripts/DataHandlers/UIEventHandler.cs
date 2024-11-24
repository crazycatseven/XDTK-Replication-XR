using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class UIEventHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "UIEvent" };

    [System.Serializable]
    public class UIEventMapping
    {
        public string elementId;
        public UnityEvent<string> onEventReceived;
    }

    [SerializeField]
    private List<UIEventMapping> eventMappings = new List<UIEventMapping>();
    private Dictionary<string, UnityEvent<string>> eventCache;

    public delegate void SuperButtonEventHandler(string jsonData);
    private Dictionary<string, SuperButtonEventHandler> superButtonHandlers = new Dictionary<string, SuperButtonEventHandler>();

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

        try 
        {
            string jsonData = System.Text.Encoding.UTF8.GetString(data);
            var eventData = UIEventProvider.UIEventData.FromJson(jsonData);
            Debug.Log($"Received UI Event - Type: {eventData.EventType}, ID: {eventData.ElementId}, Value: {eventData.Value}");

            if (superButtonHandlers.TryGetValue(eventData.ElementId, out var superButtonHandler))
            {
                Debug.Log($"调用超级按钮事件处理器: {jsonData}");
                superButtonHandler(jsonData);
            }
            else if (eventCache.TryGetValue(eventData.ElementId, out var eventCallback))
            {
                eventCallback.Invoke(eventData.Value);
            }
            else
            {
                Debug.LogWarning($"No handler found for UI element ID: {eventData.ElementId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling UI event: {e.Message}");
        }
    }

    public void AddSuperButtonHandler(string elementId, SuperButtonEventHandler handler)
    {
        superButtonHandlers[elementId] = handler;
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