using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ForceGrab : MonoBehaviour
{

    [SerializeField]
    private HandGrabInteractor _handGrabInteractor;

    [SerializeField]
    private Transform _pinchPoint;

    [SerializeField]
    private OVRSkeleton _handSkeleton;

    [SerializeField]
    private float releaseThreshold = 0.05f;

    [SerializeField]
    private GameObject _targetObject;
    

    private bool _isGrabbing = false;
    private GameObject _currentGrabbedObject;
    private Transform _indexTip;
    private Transform _thumbTip;
    private bool _bonesInitialized = false;

    private HandGrabInteractable _currentGrabbedInteractable;

    void Start()
    {
    
    }

    private void Update()
    {
        UpdateBoneReferences();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_isGrabbing)
            {
                TestGrab();
            }
        }

        if (_isGrabbing && _currentGrabbedObject != null)
        {
            // UpdateGrabbedObjectPosition();
            CheckForRelease();
        }
    }

    private void UpdateBoneReferences()
    {
        if (_handSkeleton == null || _bonesInitialized) return;

        if (_handSkeleton.Bones != null && _handSkeleton.Bones.Count > 0)
        {
            foreach (var bone in _handSkeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    _indexTip = bone.Transform;
                }
                else if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                {
                    _thumbTip = bone.Transform;
                }
            }

            if (_indexTip != null && _thumbTip != null)
            {
                _bonesInitialized = true;
            }
        }
    }

    private void UpdateGrabbedObjectPosition()
    {
        if (_pinchPoint == null) return;

        // _currentGrabbedObject.transform.position = _pinchPoint.position;

        Debug.Log($"_handGrabInteractor.CanSelect: {_handGrabInteractor.CanSelect(_currentGrabbedInteractable)}");

        _handGrabInteractor.ForceSelect(_currentGrabbedInteractable);
    }

    private void CheckForRelease()
    {
        if (!_bonesInitialized) return;

        float distance = Vector3.Distance(_indexTip.position, _thumbTip.position);
        if (distance > releaseThreshold)
        {
            ReleaseObject();
        }
    }

    public void GrabObject(GameObject obj)
    {
        if (_isGrabbing || _pinchPoint == null) return;

        _currentGrabbedObject = obj;
        _currentGrabbedInteractable = obj.GetComponentInChildren<HandGrabInteractable>();
        Debug.Log($"Grabbed interactable: {_currentGrabbedInteractable.name}");
        _isGrabbing = true;
        UpdateGrabbedObjectPosition();
    }

    private void ReleaseObject()
    {
        if (!_isGrabbing) return;

        _isGrabbing = false;
        _currentGrabbedObject = null;
        _currentGrabbedInteractable = null;
    }

    private void TestGrab()
    {
        if (_targetObject != null)
        {
            GrabObject(_targetObject);
        }
    }
}
