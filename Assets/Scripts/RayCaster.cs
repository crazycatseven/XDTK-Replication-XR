using UnityEngine;
using System.Collections.Generic;

public class RayCaster : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private LineRenderer _rayVisualizer;
    [SerializeField] private Color _collisionColor = Color.red;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private float _maxRayDistance = 100f;

    private Gradient _defaultGradient;
    private GameObject _currentHitTarget;
    private bool _isInitialized;
    private bool _isActive;
    private bool _isColliding;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (!TryGetComponent(out _rayVisualizer))
        {
            Debug.LogError($"[{nameof(RayCaster)}] Missing LineRenderer component!");
            return;
        }

        _defaultGradient = _rayVisualizer.colorGradient;
        _isInitialized = true;

        _isActive = false;
    }

    private void Update()
    {
        if (!_isInitialized || !_isActive) return;

        DetectCollision();
    }

    private void DetectCollision()
    {
        Vector3 rayStart = _rayVisualizer.GetPosition(0);
        Vector3 rayEnd = _rayVisualizer.GetPosition(1);
        Vector3 rayDirection = (rayEnd - rayStart).normalized;

        bool hitDetected = Physics.Raycast(rayStart, rayDirection, out RaycastHit hitInfo, _maxRayDistance, _targetLayers);

        if (hitDetected != _isColliding || (hitDetected && hitInfo.collider.gameObject != _currentHitTarget))
        {
            if (hitDetected)
            {
                HandleCollision(hitInfo);
            }
            else
            {
                ResetRayVisualization();
            }
            _isColliding = hitDetected;
        }
    }

    private void HandleCollision(RaycastHit hitInfo)
    {
        _currentHitTarget = hitInfo.collider.gameObject;
        UpdateRayColor(_collisionColor);
    }

    private void ResetRayVisualization()
    {
        if (_rayVisualizer.colorGradient != _defaultGradient)
        {
            _rayVisualizer.colorGradient = _defaultGradient;
        }
        _currentHitTarget = null;
    }

    private void UpdateRayColor(Color color)
    {
        if (_rayVisualizer.colorGradient.colorKeys[0].color == color) return;

        var gradient = new Gradient();
        var colorKeys = _rayVisualizer.colorGradient.colorKeys;
        var alphaKeys = _rayVisualizer.colorGradient.alphaKeys;

        colorKeys[0].color = color;
        gradient.SetKeys(colorKeys, alphaKeys);

        _rayVisualizer.colorGradient = gradient;
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        _rayVisualizer.enabled = isActive;

        if (!isActive)
        {
            ResetRayVisualization();
            _currentHitTarget = null;
            _isColliding = false;
        }
    }

    public bool IsActive() => _isActive;

    public GameObject GetCurrentTarget() => _currentHitTarget;
}