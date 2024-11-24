using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using System.Collections;

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

    [System.Serializable]
    public class SuperButtonMapping : UIElementMapping
    {
        public Button element;          // 基础按钮组件
        public float holdThreshold = 0.2f;  // 判定为长按的时间阈��
        public float dragThreshold = 5f;    // 判定为拖拽的最小距离
    }

    [SerializeField]
    private List<ButtonMapping> buttonMappings = new List<ButtonMapping>();
    [SerializeField]
    private List<SliderMapping> sliderMappings = new List<SliderMapping>();
    [SerializeField]
    private List<InputFieldMapping> inputFieldMappings = new List<InputFieldMapping>();
    [SerializeField]
    private List<ToggleMapping> toggleMappings = new List<ToggleMapping>();
    [SerializeField]
    private List<SuperButtonMapping> superButtonMappings = new List<SuperButtonMapping>();

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
                    OnUIElementTriggered(currentMapping.elementId,
                        currentMapping.eventType ?? UIEventProvider.EventTypes.ButtonClick,
                        currentMapping.defaultValue));
            }
        }

        foreach (var mapping in sliderMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onValueChanged.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId,
                        currentMapping.eventType ?? UIEventProvider.EventTypes.SliderChange,
                        value.ToString()));
            }
        }

        foreach (var mapping in inputFieldMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onEndEdit.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId,
                        currentMapping.eventType ?? UIEventProvider.EventTypes.InputFieldChange,
                        value));
            }
        }

        foreach (var mapping in toggleMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                mapping.element.onValueChanged.AddListener((value) =>
                    OnUIElementTriggered(currentMapping.elementId,
                        currentMapping.eventType ?? UIEventProvider.EventTypes.ToggleChange,
                        value.ToString()));
            }
        }

        InitializeSuperButtons();
    }

    private void InitializeSuperButtons()
    {
        foreach (var mapping in superButtonMappings)
        {
            if (mapping.element != null)
            {
                var currentMapping = mapping;
                var buttonState = new SuperButtonState();

                var eventTrigger = mapping.element.GetComponent<EventTrigger>()
                    ?? mapping.element.gameObject.AddComponent<EventTrigger>();

                AddEventTrigger(eventTrigger, EventTriggerType.PointerDown, (data) =>
                {
                    var pointerData = (PointerEventData)data;
                    buttonState.pressStartPos = pointerData.position;
                    buttonState.lastDragPos = buttonState.pressStartPos;
                    buttonState.pressStartTime = Time.time;
                    buttonState.isDragging = false;
                    buttonState.isHolding = false;

                    buttonState.holdCoroutine = StartCoroutine(HoldDetection(currentMapping, buttonState));
                });

                AddEventTrigger(eventTrigger, EventTriggerType.Drag, (data) =>
                {
                    var pointerData = (PointerEventData)data;
                    Vector2 dragDelta = pointerData.position - buttonState.lastDragPos;
                    Vector2 totalDelta = pointerData.position - buttonState.pressStartPos;

                    if (!buttonState.isDragging && totalDelta.magnitude > mapping.dragThreshold)
                    {
                        buttonState.isDragging = true;
                        buttonState.isHolding = false;

                        if (buttonState.holdCoroutine != null)
                        {
                            StopCoroutine(buttonState.holdCoroutine);
                            buttonState.holdCoroutine = null;
                        }
                    }

                    if (buttonState.isDragging)
                    {
                        string dragData = JsonUtility.ToJson(new DragEventData
                        {
                            totalDelta = totalDelta,
                            currentDelta = dragDelta,
                            position = pointerData.position
                        });

                        OnUIElementTriggered(currentMapping.elementId,
                            UIEventProvider.EventTypes.SuperButtonDrag,
                            dragData);
                    }

                    buttonState.lastDragPos = pointerData.position;
                });

                AddEventTrigger(eventTrigger, EventTriggerType.PointerUp, (data) =>
                {
                    var pointerData = (PointerEventData)data;
                    float pressDuration = Time.time - buttonState.pressStartTime;
                    Vector2 totalDelta = pointerData.position - buttonState.pressStartPos;

                    if (buttonState.holdCoroutine != null)
                    {
                        StopCoroutine(buttonState.holdCoroutine);
                        buttonState.holdCoroutine = null;
                    }

                    if (!buttonState.isDragging && !buttonState.isHolding &&
                        pressDuration < mapping.holdThreshold)
                    {
                        OnUIElementTriggered(currentMapping.elementId,
                            UIEventProvider.EventTypes.SuperButtonClick,
                            currentMapping.defaultValue);
                    }

                    if (buttonState.isDragging || buttonState.isHolding)
                    {
                        string releaseData = JsonUtility.ToJson(new ReleaseEventData
                        {
                            totalDelta = totalDelta,
                            duration = pressDuration,
                            finalPosition = pointerData.position
                        });

                        OnUIElementTriggered(currentMapping.elementId,
                            UIEventProvider.EventTypes.SuperButtonRelease,
                            releaseData);
                    }

                    buttonState.isDragging = false;
                    buttonState.isHolding = false;
                });
            }
        }
    }

    // 用于追踪每个按钮状态的类
    private class SuperButtonState
    {
        public Vector2 pressStartPos;
        public Vector2 lastDragPos;
        public float pressStartTime;
        public bool isDragging;
        public bool isHolding;
        public Coroutine holdCoroutine;  // 添加对协程的引用
    }

    // 用于序列化拖拽事件数据的类
    [Serializable]
    private class DragEventData
    {
        public Vector2 totalDelta;
        public Vector2 currentDelta;
        public Vector2 position;
    }

    // 用于序列化释放事件数据的类
    [Serializable]
    private class ReleaseEventData
    {
        public Vector2 totalDelta;
        public float duration;
        public Vector2 finalPosition;
    }

    // 辅助方法：添加事件触发器
    private void AddEventTrigger(EventTrigger eventTrigger, EventTriggerType triggerType,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry();
        entry.eventID = triggerType;
        entry.callback.AddListener(action);
        eventTrigger.triggers.Add(entry);
    }

    private IEnumerator HoldDetection(SuperButtonMapping mapping, SuperButtonState state)
    {
        yield return new WaitForSeconds(mapping.holdThreshold);

        if (!state.isDragging)
        {
            state.isHolding = true;
            OnUIElementTriggered(mapping.elementId,
                UIEventProvider.EventTypes.SuperButtonHold,
                mapping.defaultValue);
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