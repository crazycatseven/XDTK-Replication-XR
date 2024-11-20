using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIEventController : MonoBehaviour
{
    [System.Serializable]
    public abstract class UIElementMapping
    {
        public string elementId;
        public string eventType;
        [TextArea]
        public string defaultValue;
    }

    [System.Serializable]
    public class ButtonMapping : UIElementMapping
    {
        public Button element;
    }

    [System.Serializable]
    public class SliderMapping : UIElementMapping
    {
        public Slider element;
    }

    [System.Serializable]
    public class InputFieldMapping : UIElementMapping
    {
        public TMP_InputField element;
    }

    [System.Serializable]
    public class ToggleMapping : UIElementMapping
    {
        public Toggle element;
    }

    [SerializeField]
    private List<ButtonMapping> buttonMappings = new List<ButtonMapping>();
    [SerializeField]
    private List<SliderMapping> sliderMappings = new List<SliderMapping>();
    [SerializeField]
    private List<InputFieldMapping> inputFieldMappings = new List<InputFieldMapping>();
    [SerializeField]
    private List<ToggleMapping> toggleMappings = new List<ToggleMapping>();

    private UIEventProvider eventProvider;

    private void Start()
    {
        eventProvider = GetComponent<UIEventProvider>();
        if (eventProvider == null)
        {
            Debug.LogError("UIEventProvider component not found!");
            return;
        }

        InitializeEventListeners();
    }

    private void InitializeEventListeners()
    {
        foreach (var mapping in buttonMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onClick.AddListener(() =>
                    OnUIElementTriggered(currentMapping.elementId, currentMapping.eventType ?? "ButtonClick", currentMapping.defaultValue));
            }
        }

        foreach (var mapping in sliderMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onValueChanged.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId, currentMapping.eventType ?? "SliderChange", value.ToString()));
            }
        }

        foreach (var mapping in inputFieldMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onEndEdit.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId, currentMapping.eventType ?? "InputFieldChange", value));
            }
        }

        foreach (var mapping in toggleMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onValueChanged.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId, currentMapping.eventType ?? "ToggleChange", value.ToString()));
            }
        }
    }

    private void OnUIElementTriggered(string elementId, string eventType, string value)
    {
        if (eventProvider != null && eventProvider.IsEnabled)
        {
            eventProvider.SendUIEvent(eventType, elementId, value);
            Debug.Log($"UI Event sent - Type: {eventType}, ID: {elementId}, Value: {value}");
        }
    }
}