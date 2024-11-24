using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float distance = 2f; // 与摄像机的距离
    // Start is called before the first frame update
    void Start()
    {
        targetCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // 直接设置位置到摄像机前方固定距离
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distance;

        // 只旋转Y轴,保持XZ轴不变
        float yAngle = targetCamera.transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yAngle + 180, 0); // 加180度让物体面向摄像机
    }

}

