using UnityEngine;
using static PinchSpawnDelegates;
using System;

[RequireComponent(typeof(PinchSpawnHandler))]
public class HandFollowSpawnHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rightHandGrabPoint;
    [SerializeField] private Transform phoneScreenReference;

    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;

    public GetSpawnPrefabDelegate GetSpawnPrefab { get; set; }

    private PinchSpawnHandler pinchHandler;
    private GameObject currentSpawnedObject;
    private Transform meshObjectTransform;
    private Vector3 targetScale;
    private float initialPinchDistance;
    private bool isSpawning = false;

    [Header("Settings")]
    [SerializeField] private float scaleSpeed = 5f;

    private Vector3 currentTargetScale;

    private void Start()
    {
        pinchHandler = GetComponent<PinchSpawnHandler>();
        pinchHandler.OnPinchReadyForSpawn += HandlePinchReadyForSpawn;
        pinchHandler.OnPinchUpdate += HandlePinchUpdate;
        pinchHandler.OnPinchEnd += HandlePinchEnd;
        pinchHandler.OnPinchCancelled += HandlePinchCancelled;
    }


    private void HandlePinchReadyForSpawn(string value, Vector2 pos1, Vector2 pos2)
    {
        var result = GetSpawnPrefab?.Invoke(value);
        if (!result.HasValue || result.Value.prefab == null)
        {
            Debug.LogWarning($"Invalid prefab for value: {value}");
            return;
        }

        var (prefab, scale) = result.Value;
        
        Transform meshObjectInPrefab = prefab.transform.Find("MeshObject");
        if (meshObjectInPrefab == null)
        {
            Debug.LogError("Prefab must have a child named 'MeshObject'");
            return;
        }
        Vector3 originalScale = meshObjectInPrefab.localScale;
        targetScale = originalScale * scale;
        
        initialPinchDistance = Vector2.Distance(pos1, pos2);

        Vector3 directionToCamera = (Camera.main.transform.position - rightHandGrabPoint.position).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera);

        currentSpawnedObject = Instantiate(prefab, rightHandGrabPoint.position, spawnRotation);
        
        meshObjectTransform = currentSpawnedObject.transform.Find("MeshObject");
        if (meshObjectTransform != null)
        {
            meshObjectTransform.localScale = Vector3.zero;
        }
        
        isSpawning = true;

        // 播放音效
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, rightHandGrabPoint.position);
        }

        // 如果是视频，设置时间
        if (value.StartsWith("video", StringComparison.OrdinalIgnoreCase))
        {
            currentSpawnedObject.transform.Rotate(0, 180, 0);
            string timeStr = value.Substring(5); // 移除 "video" 前缀
            if (float.TryParse(timeStr, out float videoTime))
            {
                var videoController = currentSpawnedObject.transform.Find("MeshObject").GetComponent<VideoController>();
                if (videoController != null)
                {
                    videoController.SetVideoTime(videoTime);
                }
            }
        }
    }

    private void HandlePinchUpdate(Vector2 pos1, Vector2 pos2, string value)
    {
        if (currentSpawnedObject == null || !isSpawning || meshObjectTransform == null) return;

        float currentPinchDistance = Vector2.Distance(pos1, pos2);
        float progress = currentPinchDistance / initialPinchDistance;
        progress = Mathf.Clamp01(1 - progress);

        currentTargetScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
        
        meshObjectTransform.localScale = Vector3.Lerp(
            meshObjectTransform.localScale, 
            currentTargetScale, 
            Time.deltaTime * scaleSpeed
        );
    }

    private void HandlePinchEnd(Vector2 pos1, Vector2 pos2, string value)
    {
        if (currentSpawnedObject != null && isSpawning && meshObjectTransform != null)
        {
            meshObjectTransform.localScale = targetScale;
            isSpawning = false;
            currentSpawnedObject = null;
            meshObjectTransform = null;
        }
    }

    private void HandlePinchCancelled()
    {
        isSpawning = false;
        currentSpawnedObject = null;
        meshObjectTransform = null;
    }

    private void OnDestroy()
    {
        if (pinchHandler != null)
        {
            pinchHandler.OnPinchReadyForSpawn -= HandlePinchReadyForSpawn;
            pinchHandler.OnPinchUpdate -= HandlePinchUpdate;
            pinchHandler.OnPinchEnd -= HandlePinchEnd;
            pinchHandler.OnPinchCancelled -= HandlePinchCancelled;
        }
    }
}