using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PinchSpawnHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScreenGestureHandler gestureHandler;

    [Header("Pinch Settings")]
    [SerializeField] private float minPinchDistance = 200f;
    [SerializeField] private float scaleThreshold = 0.5f;

    private float initialPinchDistance;
    private bool isValidPinch;
    private string currentGestureValue;

    public event System.Action<Vector2, Vector2, string> OnPinchStart;
    public event System.Action<Vector2, Vector2, string> OnPinchUpdate;
    public event System.Action<string> OnValidPinchEnd;
    public event System.Action OnPinchCancelled;
    public event System.Action<string, Vector2, Vector2> OnPinchReadyForSpawn;

    private void Start()
    {
        if (gestureHandler == null)
        {
            gestureHandler = GetComponent<ScreenGestureHandler>();
        }

        if (gestureHandler != null)
        {
            gestureHandler.onPinchStart.AddListener(HandlePinchStart);
            gestureHandler.onPinchUpdate.AddListener(HandlePinchUpdate);
            gestureHandler.onPinchEnd.AddListener(HandlePinchEnd);
        }
        else
        {
            Debug.LogError("ScreenGestureHandler not found!");
        }
    }

    private void OnDestroy()
    {
        if (gestureHandler != null)
        {
            gestureHandler.onPinchStart.RemoveListener(HandlePinchStart);
            gestureHandler.onPinchUpdate.RemoveListener(HandlePinchUpdate);
            gestureHandler.onPinchEnd.RemoveListener(HandlePinchEnd);
        }
    }

    // 改名为 HandlePinchStart
    private void HandlePinchStart(Vector2 pos1, Vector2 pos2, string value)
    {
        initialPinchDistance = Vector2.Distance(pos1, pos2);
        currentGestureValue = value;
        isValidPinch = false;

        // 触发开始事件
        OnPinchStart?.Invoke(pos1, pos2, value);
    }

    // 改名为 HandlePinchUpdate
    private void HandlePinchUpdate(Vector2 pos1, Vector2 pos2, string value)
    {
        float currentDistance = Vector2.Distance(pos1, pos2);
        float scaleRatio = currentDistance / initialPinchDistance;

        if (!isValidPinch && scaleRatio <= scaleThreshold)
        {
            isValidPinch = true;
            OnPinchReadyForSpawn?.Invoke(currentGestureValue, pos1, pos2);
        }

        // 触发更新事件
        OnPinchUpdate?.Invoke(pos1, pos2, value);
    }

    // 改名为 HandlePinchEnd
    private void HandlePinchEnd()
    {
        if (isValidPinch)
        {
            OnValidPinchEnd?.Invoke(currentGestureValue);
        }
        else
        {
            OnPinchCancelled?.Invoke();
        }

        isValidPinch = false;
    }
}