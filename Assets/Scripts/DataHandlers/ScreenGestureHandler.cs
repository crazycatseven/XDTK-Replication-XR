using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class ScreenGestureHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes =>
        new List<string> { "ScreenGesture" };

    [Serializable]
    public class PinchEvent : UnityEvent<Vector2, Vector2, string> { }  // position1, position2, value

    public PinchEvent onPinchStart;
    public PinchEvent onPinchUpdate;
    public UnityEvent onPinchEnd;

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "ScreenGesture") return;

        string jsonData = System.Text.Encoding.UTF8.GetString(data);
        var gestureData = JsonUtility.FromJson<ScreenGestureProvider.PinchData>(jsonData);

        switch (gestureData.eventType)
        {
            case ScreenGestureProvider.EventTypes.PinchStart:
                onPinchStart?.Invoke(gestureData.touch1Pos, gestureData.touch2Pos, gestureData.value);
                break;

            case ScreenGestureProvider.EventTypes.PinchUpdate:
                onPinchUpdate?.Invoke(gestureData.touch1Pos, gestureData.touch2Pos, gestureData.value);
                break;

            case ScreenGestureProvider.EventTypes.PinchEnd:
                onPinchEnd?.Invoke();
                break;
        }
    }
}