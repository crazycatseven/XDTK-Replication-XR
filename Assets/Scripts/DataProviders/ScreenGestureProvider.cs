using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScreenGestureProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "ScreenGesture";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    public static class EventTypes
    {
        public const string PinchStart = "PinchStart";
        public const string PinchUpdate = "PinchUpdate";
        public const string PinchEnd = "PinchEnd";
    }

    [SerializeField]
    private float minPinchDistance = 10f; // 最小捏合距离
    private bool isPinching = false;
    private Vector2 pinchStartPos1, pinchStartPos2;
    private float pinchStartDistance;

    // 用于暂时禁用其他UI交互
    private EventSystem eventSystem;
    private GraphicRaycaster[] raycasters;

    [Serializable]
    public class PinchData
    {
        public string eventType;    // 事件类型
        public Vector2 touch1Pos;   // 第一个手指位置
        public Vector2 touch2Pos;   // 第二个手指位置
        public string value;        // 添加value字段
    }

    // 添加一个委托用于获取value
    public delegate string GetGestureValueDelegate();
    public GetGestureValueDelegate GetGestureValue { get; set; }

    private void Start()
    {
        eventSystem = EventSystem.current;
        raycasters = FindObjectsOfType<GraphicRaycaster>();
    }

    private void Update()
    {
        if (!IsEnabled) return;

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (!isPinching &&
                (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began))
            {
                // 开始捏合
                StartPinch(touch1, touch2);
            }
            else if (isPinching &&
                     (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved))
            {
                // 捏合更新
                UpdatePinch(touch1, touch2);
            }
            else if (isPinching &&
                     (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended ||
                      touch1.phase == TouchPhase.Canceled || touch2.phase == TouchPhase.Canceled))
            {
                // 结束捏合
                EndPinch();
            }
        }
        else if (isPinching)
        {
            // 如果捏合过程中失去了触摸点
            EndPinch();
        }
    }

    private void StartPinch(Touch touch1, Touch touch2)
    {
        isPinching = true;
        pinchStartPos1 = touch1.position;
        pinchStartPos2 = touch2.position;
        pinchStartDistance = Vector2.Distance(pinchStartPos1, pinchStartPos2);

        DisableUIInteraction();
        SendPinchEvent(EventTypes.PinchStart, touch1, touch2);
    }

    private void UpdatePinch(Touch touch1, Touch touch2)
    {
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float deltaDistance = currentDistance - pinchStartDistance;

        if (Mathf.Abs(deltaDistance) > minPinchDistance)
        {
            SendPinchEvent(EventTypes.PinchUpdate, touch1, touch2);
        }
    }

    private void EndPinch()
    {
        if (!isPinching) return;

        isPinching = false;
        EnableUIInteraction();

        var pinchData = new PinchData
        {
            eventType = EventTypes.PinchEnd,
            touch1Pos = Vector2.zero,
            touch2Pos = Vector2.zero,
            value = GetGestureValue?.Invoke() ?? string.Empty
        };
        
        SendEvent(EventTypes.PinchEnd, pinchData);
        OnScreenGestureTriggered(EventTypes.PinchEnd, pinchData);
    }

    private void SendPinchEvent(string eventType, Touch touch1, Touch touch2)
    {
        var pinchData = new PinchData
        {
            eventType = eventType,
            touch1Pos = touch1.position,
            touch2Pos = touch2.position,
            value = GetGestureValue?.Invoke() ?? string.Empty
        };

        SendEvent(eventType, pinchData);
        OnScreenGestureTriggered(eventType, pinchData);
    }

    private void SendEvent(string eventType, PinchData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, bytes);
    }

    private void DisableUIInteraction()
    {
        // 禁用EventSystem
        if (eventSystem != null)
            eventSystem.enabled = false;

        // 禁用所有GraphicRaycaster
        foreach (var raycaster in raycasters)
        {
            if (raycaster != null)
                raycaster.enabled = false;
        }
    }

    private void EnableUIInteraction()
    {
        // 重新启用EventSystem
        if (eventSystem != null)
            eventSystem.enabled = true;

        // 重新启用所有GraphicRaycaster
        foreach (var raycaster in raycasters)
        {
            if (raycaster != null)
                raycaster.enabled = true;
        }
    }

    private void OnScreenGestureTriggered(string eventType, PinchData data)
    {
        Debug.Log($"ScreenGestureProvider: {eventType} triggered");
    }
}