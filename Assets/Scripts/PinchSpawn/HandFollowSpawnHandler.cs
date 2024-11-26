using UnityEngine;
using static PinchSpawnDelegates;

[RequireComponent(typeof(PinchSpawnHandler))]
public class HandFollowSpawnHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rightHandGrabPoint;
    [SerializeField] private Transform phoneScreenReference;
    
    public GetSpawnPrefabDelegate GetSpawnPrefab { get; set; }

    private PinchSpawnHandler pinchHandler;
    private GameObject currentSpawnedObject;
    private Transform meshObjectTransform;
    private Vector3 targetScale;
    private float initialPinchDistance;
    private bool isSpawning = false;

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

        Quaternion spawnRotation = phoneScreenReference != null 
            ? phoneScreenReference.rotation * Quaternion.Euler(-90f, 0f, 0f) 
            : rightHandGrabPoint.rotation;

        currentSpawnedObject = Instantiate(prefab, rightHandGrabPoint.position, spawnRotation);
        
        meshObjectTransform = currentSpawnedObject.transform.Find("MeshObject");
        if (meshObjectTransform != null)
        {
            meshObjectTransform.localScale = Vector3.zero;
        }
        
        isSpawning = true;
    }

    private void HandlePinchUpdate(Vector2 pos1, Vector2 pos2, string value)
    {
        if (currentSpawnedObject == null || !isSpawning || meshObjectTransform == null) return;

        float currentPinchDistance = Vector2.Distance(pos1, pos2);
        float progress = currentPinchDistance / initialPinchDistance;
        progress = Mathf.Clamp01(1 - progress);

        Vector3 currentScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
        meshObjectTransform.localScale = currentScale;
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