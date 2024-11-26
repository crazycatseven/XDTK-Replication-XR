using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class PinchEventData
{
    public string type;         // pinch event type
    public Vector2 touch1;      // first touch point
    public Vector2 touch2;      // second touch point
    public string value;        // business data (e.g. selected item ID)
}

public class ScreenGestureProvider : MonoBehaviour, IDataProvider
{

    #region Public Properties
    public string DataType => "ScreenGesture";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;
    #endregion

    #region Gesture Data Structure

    private static class PinchTypes
    {
        public const string Start = "PinchStart";
        public const string Update = "PinchUpdate";
        public const string End = "PinchEnd";
    }
    #endregion

    #region Pinch Gesture Properties
    [SerializeField]
    private float minPinchDistance = 10f; // 最小捏合距离
    private bool isPinching = false;
    private Vector2 pinchStartPos1, pinchStartPos2;
    private float pinchStartDistance;
    #endregion

    #region UI Interaction Properties
    private EventSystem eventSystem;
    private GraphicRaycaster[] raycasters;
    #endregion

    #region Public Events
    public Action<Vector2, Vector2, string> OnPinchStart; // touch1, touch2, value
    public Action<Vector2, Vector2, string> OnPinchUpdate; // touch1, touch2, value
    public Action<Vector2, Vector2, string> OnPinchEnd; // touch1, touch2, value
    #endregion



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
                HandlePinchStart(touch1, touch2);
            }
            else if (isPinching &&
                     (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved))
            {
                HandlePinchUpdate(touch1, touch2);
            }
            else if (isPinching &&
                     (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended ||
                      touch1.phase == TouchPhase.Canceled || touch2.phase == TouchPhase.Canceled))
            {
                HandlePinchEnd(touch1, touch2);
            }
        }
    }

    public void HandlePinchStart(Touch touch1, Touch touch2, string value = "")
    {
        isPinching = true;
        pinchStartPos1 = touch1.position;
        pinchStartPos2 = touch2.position;
        pinchStartDistance = Vector2.Distance(pinchStartPos1, pinchStartPos2);

        DisableUIInteraction();
        SendPinchEvent(PinchTypes.Start, touch1, touch2, value);
    }

    public void HandlePinchUpdate(Touch touch1, Touch touch2, string value = "")
    {
        float currentDistance = Vector2.Distance(touch1.position, touch2.position);
        float deltaDistance = currentDistance - pinchStartDistance;

        if (Mathf.Abs(deltaDistance) > minPinchDistance)
        {
            SendPinchEvent(PinchTypes.Update, touch1, touch2, value);
        }
    }

    public void HandlePinchEnd(Touch touch1, Touch touch2, string value = "")
    {
        if (!isPinching) return;
        isPinching = false;

        EnableUIInteraction();
        SendPinchEvent(PinchTypes.End, touch1, touch2, value);
    }


    public void SendPinchEvent(string eventType, Touch touch1, Touch touch2, string value = "")
    {
        var pinchData = new PinchEventData
        {
            type = eventType,
            touch1 = touch1.position,
            touch2 = touch2.position,
            value = value
        };

        string jsonData = JsonUtility.ToJson(pinchData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, bytes);

        Debug.Log("SendPinchEvent: " + eventType + " Value: " + value);

        switch (eventType)
        {
            case PinchTypes.Start:
                OnPinchStart?.Invoke(touch1.position, touch2.position, value);
                break;
            case PinchTypes.Update:
                OnPinchUpdate?.Invoke(touch1.position, touch2.position, value);
                break;
            case PinchTypes.End:
                OnPinchEnd?.Invoke(touch1.position, touch2.position, value);
                break;
        }
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

}