using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class WatchAttachment : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform _wristTransform;

    [SerializeField]
    private HandGrabInteractable _grabInteractable;

    [Header("Settings")]
    [SerializeField]
    private Vector3 _positionOffset = Vector3.zero;

    [SerializeField]
    private Vector3 _rotationOffset = Vector3.zero;

    [SerializeField]
    private float _attachDistance = 0.1f;

    [SerializeField]
    private float _transitionSpeed = 5f;

    [SerializeField]
    private float _attachDuration = 0.5f;
    private float _attachStartTime;
    private Vector3 _attachStartPosition;
    private Quaternion _attachStartRotation;

    private bool _isGrabbed = false;
    private bool _isAttaching = false;
    private bool _isAttached = false;

    private void Start()
    {
        if (_grabInteractable == null)
        {
            _grabInteractable = GetComponent<HandGrabInteractable>();
        }

        if (_wristTransform == null)
        {
            _wristTransform = GameObject.Find("HandWristPointLeft")?.transform;
        }

        if (_grabInteractable != null)
        {
            _grabInteractable.WhenSelectingInteractorAdded.Action += OnGrabbed;
            _grabInteractable.WhenSelectingInteractorRemoved.Action += OnReleased;
        }
    }

    private void Update()
    {
        if (!_wristTransform || _isGrabbed) return;

        Vector3 targetPosition = _wristTransform.position + _wristTransform.TransformDirection(_positionOffset);
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (!_isAttached && !_isAttaching && distanceToTarget <= _attachDistance)
        {
            StartAttaching();
        }

        if (_isAttaching)
        {
            UpdateAttachTransition();
        }
        else if (_isAttached)
        {
            UpdateAttachedPosition();
        }
    }

    private void StartAttaching()
    {
        _isAttaching = true;
        _attachStartTime = Time.time;
        _attachStartPosition = transform.position;
        _attachStartRotation = transform.rotation;
    }

    private void UpdateAttachTransition()
    {
        float elapsedTime = Time.time - _attachStartTime;
        float t = Mathf.Clamp01(elapsedTime / _attachDuration);
        
        Vector3 targetPosition = _wristTransform.position + _wristTransform.TransformDirection(_positionOffset);
        Quaternion targetRotation = _wristTransform.rotation * Quaternion.Euler(_rotationOffset);

        transform.position = Vector3.Lerp(_attachStartPosition, targetPosition, t);
        transform.rotation = Quaternion.Lerp(_attachStartRotation, targetRotation, t);

        if (t >= 1.0f)
        {
            _isAttaching = false;
            _isAttached = true;
        }
    }

    private void UpdateAttachedPosition()
    {
        transform.position = _wristTransform.position + _wristTransform.TransformDirection(_positionOffset);
        transform.rotation = _wristTransform.rotation * Quaternion.Euler(_rotationOffset);
    }

    private void OnGrabbed(IHandGrabInteractor interactor)
    {
        _isGrabbed = true;
        _isAttached = false;
        _isAttaching = false;
    }

    private void OnReleased(IHandGrabInteractor interactor)
    {
        _isGrabbed = false;
    }

    private void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.WhenSelectingInteractorAdded.Action -= OnGrabbed;
            _grabInteractable.WhenSelectingInteractorRemoved.Action -= OnReleased;
        }
    }
}