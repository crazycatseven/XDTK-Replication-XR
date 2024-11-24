using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager : MonoBehaviour
{

    [Header("Required Components")]
    [SerializeField] private ARCameraHandler arCameraHandler;
    [SerializeField] private GameObject phoneParent;
    [SerializeField] private GameObject originAnchor;
    [SerializeField] private GameObject crosshair;

    [Header("Ray Visualization")]
    [SerializeField] private RayVisualizer rayVisualizer;
    [SerializeField] private RayInteractor rayInteractor;

    private bool isAligned = false;

    void Start()
    {
        if (rayInteractor != null)
        {
            rayInteractor = GetComponent<RayInteractor>();
        }

        if (rayVisualizer != null)
        {
            rayVisualizer = GetComponent<RayVisualizer>();
        }
    }

    private void Update()
    {
        if (!isAligned) return;

        // 更新射线交互和可视化
        bool hasHit = rayInteractor.CheckInteraction(
            phoneParent.transform.position + phoneParent.transform.up * 0.12f,
            phoneParent.transform.up
        );

        // if (hasHit)
        // {
        //     Debug.Log($"Hit object: {rayInteractor.GetCurrentTarget().name}");
        // }

        rayVisualizer.UpdateRayPositions(
            phoneParent.transform.position, 
            phoneParent.transform.up,
            hasHit  // 传递碰撞状态
        );
    }

    // Start is called before the first frame update
    public void PerformCrosshairAlignment()
    {
        if (!ValidateComponents()) return;

        // 1. 获取手机模型在HMD空间中的位置和旋转
        Vector3 phoneHMDPosition = crosshair.transform.position;
        // 注意：可能需要根据手机模型的朝向调整这个旋转
        Quaternion phoneHMDRotation = crosshair.transform.rotation * Quaternion.Euler(0, 180, 0);

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

        isAligned = true;
        rayVisualizer.SetActive(true);
    }

    private bool ValidateComponents()
    {
        if (arCameraHandler == null)
        {
            Debug.LogError("CrosshairAlignment: ARCameraHandler reference is missing!");
            return false;
        }

        if (phoneParent == null)
        {
            Debug.LogError("CrosshairAlignment: Phone Parent reference is missing!");
            return false;
        }

        if (originAnchor == null)
        {
            Debug.LogError("CrosshairAlignment: Origin Anchor reference is missing!");
            return false;
        }

        if (crosshair == null)
        {
            Debug.LogError("CrosshairAlignment: Crosshair reference is missing!");
            return false;
        }

        return true;
    }
}
