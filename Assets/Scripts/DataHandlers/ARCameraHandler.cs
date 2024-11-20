using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class ARCameraHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "ARCameraData" };

    [SerializeField] private GameObject phoneParent;
    private Quaternion rotationOffset = Quaternion.identity;
    private Vector3 positionOffset = Vector3.zero;

    // 提供给其他系统访问的属性
    public Vector3 ReceivedPosition { get; private set; }
    public Quaternion ReceivedRotation { get; private set; }

    private Matrix4x4 transformMatrix = Matrix4x4.identity;

    [Serializable]
    public class ARCameraData
    {
        public Vector3 position;
        public Quaternion rotation;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ARCameraData FromJson(string json)
        {
            return JsonUtility.FromJson<ARCameraData>(json);
        }
    }

    public void SetTransformMatrix(Matrix4x4 matrix)
    {
        transformMatrix = matrix;
        ApplyTransform();
    }

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "ARCameraData")
        {
            Debug.LogWarning($"ARCameraHandler: Unsupported data type {dataType} received.");
            return;
        }

        string jsonData = Encoding.UTF8.GetString(data);
        ARCameraData cameraData = ARCameraData.FromJson(jsonData);
        ReceivedPosition = cameraData.position;
        ReceivedRotation = cameraData.rotation;

        ApplyTransform();
    }

    private void ApplyTransform()
    {
        if (phoneParent == null) return;

        // 将手机本地坐标转换到HMD坐标系
        Vector4 worldPos = transformMatrix * new Vector4(ReceivedPosition.x, ReceivedPosition.y, ReceivedPosition.z, 1);
        Vector3 position = new Vector3(worldPos.x, worldPos.y, worldPos.z);

        // 旋转需要考虑坐标系的变换
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(ReceivedRotation);
        Matrix4x4 transformedRotation = transformMatrix * rotationMatrix;
        Quaternion rotation = transformedRotation.rotation;

        // 应用变换
        phoneParent.transform.position = position;
        phoneParent.transform.rotation = rotation;
    }

    public void SetOffsets(Vector3 posOffset, Quaternion rotOffset)
    {
        // 从偏移构建变换矩阵
        transformMatrix = Matrix4x4.TRS(posOffset, rotOffset, Vector3.one);
        ApplyTransform();
    }
}