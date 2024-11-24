using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureAlignment : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private ARCameraHandler arCameraHandler;
    [SerializeField] private GameObject phoneParent;
    [SerializeField] private GameObject originAnchor;

    [Header("Hand Tracking References")]
    [SerializeField] private Transform index1;      // 食指第一关节
    [SerializeField] private Transform indexTip;    // 食指指尖
    [SerializeField] private Transform thumb1;      // 拇指第一关节
    [SerializeField] private Transform thumbTip;    // 拇指指尖

    private float phoneWidth = 0.064f;
    private float phoneHeight = 0.146f;

    private Matrix4x4 transformMatrix;

    public void PerformGestureAlignment()
    {
        if (!ValidateComponents()) return;

        // 1. 通过手势确定手机在HMD空间中的位置和旋转
        Vector3 phoneUp = (indexTip.position - index1.position).normalized;
        Vector3 phoneRight = (thumbTip.position - thumb1.position).normalized;
        Vector3 phoneForward = Vector3.Cross(phoneRight, phoneUp).normalized;

        // 确保正交性
        phoneRight = Vector3.Cross(phoneUp, phoneForward).normalized;

        Vector3 phoneHMDPosition = thumb1.position + phoneRight * (phoneWidth * 0.0f) + phoneUp * (phoneHeight * 0.0f);
        Quaternion phoneHMDRotation = Quaternion.LookRotation(phoneForward, phoneUp);

        // 2. 计算originAnchor的位置和旋转
        Vector3 originWorldPos = phoneHMDPosition - phoneHMDRotation * arCameraHandler.ReceivedPosition;
        Quaternion originWorldRot = phoneHMDRotation * Quaternion.Inverse(arCameraHandler.ReceivedRotation);

        // 确保originAnchor只有y轴的旋转
        Vector3 euler = originWorldRot.eulerAngles;
        originWorldRot = Quaternion.Euler(0, euler.y, 0);

        // 3. 设置originAnchor的位置和旋转
        originAnchor.transform.position = originWorldPos;
        originAnchor.transform.rotation = originWorldRot;

        // 4. 将phoneParent设置为originAnchor的子物体
        phoneParent.transform.parent = originAnchor.transform;
    }

    private bool ValidateComponents()
    {
        if (arCameraHandler == null)
        {
            Debug.LogError("GestureAlignment: ARCameraHandler reference is missing!");
            return false;
        }

        if (phoneParent == null)
        {
            Debug.LogError("GestureAlignment: Phone references are missing!");
            return false;
        }

        if (index1 == null || indexTip == null || thumb1 == null || thumbTip == null)
        {
            Debug.LogError("GestureAlignment: Hand tracking references are missing!");
            return false;
        }

        return true;
    }
}