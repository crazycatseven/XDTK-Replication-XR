using UnityEngine;

public class BoxAlignment : MonoBehaviour
{
    public Transform phoneTransform;
    public Transform boxTransform;

    private Vector3 positionCorrection;
    private Quaternion rotationCorrection;

    public void CalculateAlignment()
    {
        positionCorrection = boxTransform.position - phoneTransform.position;

        rotationCorrection = Quaternion.Inverse(phoneTransform.rotation) * boxTransform.rotation;

        Debug.Log("Alignment Complete");
        Debug.Log("Position Correction: " + positionCorrection);
        Debug.Log("Rotation Correction: " + rotationCorrection.eulerAngles);
    }

    public Vector3 ApplyPositionAlignment(Vector3 phonePosition)
    {
        return phonePosition + positionCorrection;
    }

    public Quaternion ApplyRotationAlignment(Quaternion phoneRotation)
    {
        return phoneRotation * rotationCorrection;
    }
}
