using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureAlignment : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private ARCameraHandler arCameraHandler;
    [SerializeField] private GameObject phoneParent;

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

        // 1. 获取手机在其自身坐标系中的变换矩阵
        Matrix4x4 phoneLocalMatrix = Matrix4x4.TRS(
            arCameraHandler.ReceivedPosition,
            arCameraHandler.ReceivedRotation,
            Vector3.one
        );

        // 2. 通过手势构建手机在HMD坐标系中的变换矩阵

        // 计算手机的“上”方向（食指方向）
        Vector3 phoneUp = (indexTip.position - index1.position).normalized;

        // 计算手机的“右”方向（拇指方向）
        Vector3 phoneRight = (thumbTip.position - thumb1.position).normalized;

        // 计算手机的“前”方向（右手定则）
        Vector3 phoneForward = -Vector3.Cross(phoneUp, phoneRight).normalized;

        // 确保正交性
        phoneRight = Vector3.Cross(phoneForward, phoneUp).normalized;

        // 如果旋转方向不正确，可能需要反转某些轴
        // 例如，如果手机的前方向相反，可以反转phoneForward
        // phoneForward = -phoneForward;

        // 构建旋转
        Quaternion phoneHMDRotation = Quaternion.LookRotation(phoneForward, phoneUp);

        // 计算手机在HMD坐标系中的位置
        // 假设拇指第一关节在手机的左下角
        Vector3 phoneHMDPosition = thumb1.position
            + phoneRight * (phoneWidth * 0.5f)
            + phoneUp * (phoneHeight * 0.5f);

        // 构建手机在HMD坐标系中的变换矩阵
        Matrix4x4 phoneHMDMatrix = Matrix4x4.TRS(
            phoneHMDPosition,
            phoneHMDRotation,
            Vector3.one
        );

        // 3. 计算从手机坐标系到HMD坐标系的变换矩阵
        transformMatrix = phoneHMDMatrix * phoneLocalMatrix.inverse;

        // 将变换矩阵分解为位置和旋转
        Vector3 positionOffset = transformMatrix.GetColumn(3);
        Quaternion rotationOffset = Quaternion.LookRotation(
            transformMatrix.GetColumn(2),
            transformMatrix.GetColumn(1)
        );

        // 保存变换信息
        arCameraHandler.SetTransformMatrix(transformMatrix);
        // 或者，如果需要兼容现有代码
        arCameraHandler.SetOffsets(positionOffset, rotationOffset);
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