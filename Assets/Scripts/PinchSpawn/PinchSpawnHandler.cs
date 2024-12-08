using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PinchSpawnHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScreenGestureHandler gestureHandler;

    [Header("Pinch Settings")]
    [SerializeField] private float minPinchDistance = 200f;
    [SerializeField] private float scaleThreshold = 0.4f;

    [Header("Spread Settings")]
    [SerializeField] private float spreadDistanceRatio = 1.2f;  // 展开距离比例（结束距离/开始距离）
    [SerializeField] private float minUpwardMovement = 100f;    // 最小向上移动距离

    private float initialPinchDistance;
    private bool isValidPinch;
    private string currentGestureValue;
    private Vector2 initialPos1;
    private Vector2 initialPos2;

    public event System.Action<Vector2, Vector2, string> OnPinchStart;
    public event System.Action<Vector2, Vector2, string> OnPinchUpdate;
    public event System.Action<Vector2, Vector2, string> OnPinchEnd;
    public event System.Action OnPinchCancelled;
    public event System.Action<string, Vector2, Vector2> OnPinchReadyForSpawn;
    public event System.Action<Vector2, Vector2, string> OnSpreadEnd;

    private void Start()
    {
        if (gestureHandler == null)
        {
            gestureHandler = GetComponent<ScreenGestureHandler>();
        }

        if (gestureHandler != null)
        {
            gestureHandler.onPinchStart.AddListener(new UnityAction<Vector2, Vector2, string>(HandlePinchStart));
            gestureHandler.onPinchUpdate.AddListener(new UnityAction<Vector2, Vector2, string>(HandlePinchUpdate));
            gestureHandler.onPinchEnd.AddListener(new UnityAction<Vector2, Vector2, string>(HandlePinchEnd));
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
            gestureHandler.onPinchStart.RemoveListener(new UnityAction<Vector2, Vector2, string>(HandlePinchStart));
            gestureHandler.onPinchUpdate.RemoveListener(new UnityAction<Vector2, Vector2, string>(HandlePinchUpdate));
            gestureHandler.onPinchEnd.RemoveListener(new UnityAction<Vector2, Vector2, string>(HandlePinchEnd));
        }
    }

    private void HandlePinchStart(Vector2 pos1, Vector2 pos2, string value)
    {
        initialPos1 = pos1;
        initialPos2 = pos2;
        initialPinchDistance = Vector2.Distance(pos1, pos2);
        currentGestureValue = value;
        isValidPinch = false;

        OnPinchStart?.Invoke(pos1, pos2, value);
    }

    private void HandlePinchUpdate(Vector2 pos1, Vector2 pos2, string value)
    {

        float currentDistance = Vector2.Distance(pos1, pos2);
        float scaleRatio = currentDistance / initialPinchDistance;

        if (!isValidPinch && scaleRatio <= scaleThreshold)
        {
            isValidPinch = true;
            OnPinchReadyForSpawn?.Invoke(currentGestureValue, pos1, pos2);
        }

        OnPinchUpdate?.Invoke(pos1, pos2, value);
    }

    private void HandlePinchEnd(Vector2 pos1, Vector2 pos2, string value)
    {
        // Spread Gesture Check
        float currentDistance = Vector2.Distance(pos1, pos2);
        float distanceRatio = currentDistance / initialPinchDistance;

        // Calculate average upward movement
        float avgUpwardMovement = ((initialPos1.y - pos1.y) + (initialPos2.y - pos2.y)) * 0.5f;

        // Check if the spread gesture conditions are met:
        // 1. Distance ratio meets the requirement
        // 2. Average upward movement meets the requirement
        if (distanceRatio >= spreadDistanceRatio && avgUpwardMovement >= minUpwardMovement)
        {
            Debug.Log("Valid Spread: " + value);
            OnSpreadEnd?.Invoke(pos1, pos2, value);
            return;
        }

        // Original pinch end logic
        if (isValidPinch)
        {
            OnPinchEnd?.Invoke(pos1, pos2, value);
        }
        else
        {
            OnPinchCancelled?.Invoke();
        }

        isValidPinch = false;
    }
}