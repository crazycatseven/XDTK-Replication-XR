using UnityEngine;


public class RayInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers = Physics.AllLayers;
    [SerializeField] private float _maxRayDistance = 100f;

    private GameObject _currentTarget;
    public event System.Action<GameObject> OnTargetChanged;

    public bool HasTarget => _currentTarget != null;

    public bool CheckInteraction(Vector3 origin, Vector3 direction)
    {
        LayerMask effectiveLayerMask = _targetLayers == 0 ? Physics.AllLayers : _targetLayers;
        
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, _maxRayDistance, effectiveLayerMask))
        {
            if (_currentTarget != hitInfo.collider.gameObject)
            {
                _currentTarget = hitInfo.collider.gameObject;
                OnTargetChanged?.Invoke(_currentTarget);
            }
            return true;
        }
        else if (_currentTarget != null)
        {
            _currentTarget = null;
            OnTargetChanged?.Invoke(null);
        }
        return false;
    }

    public GameObject GetCurrentTarget() => _currentTarget;
} 