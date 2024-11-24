using UnityEngine;

public class RayVisualizer : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _rayLength = 5f;
    [SerializeField] private float _rayOffset = 0.12f;

    [Header("Ray Colors")]
    [SerializeField] private Color _hitColor = Color.red;
    private Gradient _defaultGradient;

    private void Start()
    {
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        if (_lineRenderer == null) return;
        
        _lineRenderer.startWidth = 0.01f;
        _lineRenderer.endWidth = 0.01f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // 保存默认渐变
        _defaultGradient = _lineRenderer.colorGradient;
        SetActive(false);
    }

    public void UpdateRayPositions(Vector3 origin, Vector3 direction, bool hasHit = false)
    {
        if (_lineRenderer == null) return;

        Vector3 startPos = origin + direction * _rayOffset;
        Vector3 endPos = startPos + direction * _rayLength;
        
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        // 根据是否击中更新射线颜色
        if (hasHit)
        {
            UpdateRayColor(_hitColor);
        }
        else
        {
            ResetRayVisualization();
        }
    }

    private void UpdateRayColor(Color color)
    {
        if (_lineRenderer.colorGradient.colorKeys[0].color == color) return;

        var gradient = new Gradient();
        var colorKeys = _lineRenderer.colorGradient.colorKeys;
        var alphaKeys = _lineRenderer.colorGradient.alphaKeys;

        colorKeys[0].color = color;
        gradient.SetKeys(colorKeys, alphaKeys);

        _lineRenderer.colorGradient = gradient;
    }

    private void ResetRayVisualization()
    {
        if (_lineRenderer.colorGradient != _defaultGradient)
        {
            _lineRenderer.colorGradient = _defaultGradient;
        }
    }

    public void SetActive(bool isActive)
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = isActive;
            
            if (!isActive)
            {
                ResetRayVisualization();
            }
        }
    }
} 