using UnityEngine;
using static PinchSpawnDelegates;
using System;

[RequireComponent(typeof(PinchSpawnHandler))]
public class SpreadSpawnHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rightHandGrabPoint;
    [SerializeField] private Transform phoneScreenReference;
    
    [Header("Spawn Settings")]
    [SerializeField] private float targetDistanceFromCamera = 0.5f;  // 目标位置距离摄像机的距离
    [SerializeField] private float moveSpeed = 2f;                   // 移动速度
    [SerializeField] private float scaleSpeed = 5f;                  // 缩放速度
    
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;

    public GetSpawnPrefabDelegate GetSpawnPrefab { get; set; }

    private PinchSpawnHandler pinchHandler;
    private GameObject currentSpawnedObject;
    private Transform meshObjectTransform;
    private Vector3 targetPosition;
    private Vector3 targetScale;
    private bool isAnimating = false;
    private Vector3 velocity = Vector3.zero;  // 用于SmoothDamp的速度缓存
    private Vector3 scaleVelocity = Vector3.zero;  // 用于缩放的速度缓存

    private void Start()
    {
        pinchHandler = GetComponent<PinchSpawnHandler>();
        pinchHandler.OnSpreadEnd += HandleSpreadEnd;
    }

    private void HandleSpreadEnd(Vector2 pos1, Vector2 pos2, string value)
    {
        var result = GetSpawnPrefab?.Invoke(value);  // 使用传入的 value，而不是 "spread"
        if (!result.HasValue || result.Value.prefab == null)
        {
            Debug.LogWarning($"Invalid prefab for value: {value}");  // 添加 value 到警告信息
            return;
        }

        var (prefab, scale) = result.Value;

        // 检查预制体是否有MeshObject子物体
        Transform meshObjectInPrefab = prefab.transform.Find("MeshObject");
        if (meshObjectInPrefab == null)
        {
            Debug.LogError("Prefab must have a child named 'MeshObject'");
            return;
        }

        // 计算目标缩放
        Vector3 originalScale = meshObjectInPrefab.localScale;
        targetScale = originalScale * scale;

        // 计算目标位置（摄像机前方，与摄像机同高）
        Vector3 forwardProjection = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        targetPosition = Camera.main.transform.position + 
                        forwardProjection.normalized * targetDistanceFromCamera;
        targetPosition.y = Camera.main.transform.position.y; // 确保与摄像机同高

        // 计算生成位置（右手位置）和朝向
        Vector3 spawnPosition = rightHandGrabPoint.position;
        Vector3 directionToCamera = (Camera.main.transform.position - rightHandGrabPoint.position).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(directionToCamera, Vector3.up));

        // 生成物体
        currentSpawnedObject = Instantiate(prefab, spawnPosition, spawnRotation);
        meshObjectTransform = currentSpawnedObject.transform.Find("MeshObject");
        
        if (meshObjectTransform != null)
        {
            meshObjectTransform.localScale = Vector3.zero;
        }

        isAnimating = true;

        // 播放音效
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, spawnPosition);
        }

        // 如果是视频，设置时间
        if (value.StartsWith("video", StringComparison.OrdinalIgnoreCase))
        {
            string timeStr = value.Substring(5); // 移除 "video" 前缀
            // y旋转180度
            currentSpawnedObject.transform.Rotate(0, 180, 0);
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

    private void Update()
    {
        if (!isAnimating || currentSpawnedObject == null || meshObjectTransform == null)
            return;

        // 更新位置
        currentSpawnedObject.transform.position = Vector3.SmoothDamp(
            currentSpawnedObject.transform.position,
            targetPosition,
            ref velocity,
            0.3f  // 平滑时间，可以根据需要调整
        );

        // 更新缩放
        meshObjectTransform.localScale = Vector3.SmoothDamp(
            meshObjectTransform.localScale,
            targetScale,
            ref scaleVelocity,
            0.3f  // 平滑时间，可以根据需要调整
        );

        // 检查动画是否完成
        if (Vector3.Distance(currentSpawnedObject.transform.position, targetPosition) < 0.01f &&
            Vector3.Distance(meshObjectTransform.localScale, targetScale) < 0.01f)
        {
            isAnimating = false;
            currentSpawnedObject.transform.position = targetPosition;
            meshObjectTransform.localScale = targetScale;
            currentSpawnedObject = null;
            meshObjectTransform = null;
        }
    }

    private void OnDestroy()
    {
        if (pinchHandler != null)
        {
            pinchHandler.OnSpreadEnd -= HandleSpreadEnd;
        }
    }
} 