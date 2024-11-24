using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class ARCameraHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "ARCameraData" };

    [SerializeField] private GameObject phoneParent;

    // 提供给其他系统访问的属性
    public Vector3 ReceivedPosition { get; private set; }
    public Quaternion ReceivedRotation { get; private set; }

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

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "ARCameraData")
        {
            Debug.LogWarning($"ARCameraHandler: Unsupported data type {dataType} received.");
            return;
        }

        string jsonData = Encoding.UTF8.GetString(data);
        ARCameraData cameraData = ARCameraData.FromJson(jsonData);

        // 更新接收到的位置和旋转
        ReceivedPosition = cameraData.position;
        ReceivedRotation = cameraData.rotation;

        // 直接设置phoneParent的本地变换
        if (phoneParent != null)
        {
            phoneParent.transform.localPosition = ReceivedPosition;
            phoneParent.transform.localRotation = ReceivedRotation;
        }
    }

    private void OnValidate()
    {
        if (phoneParent == null)
        {
            Debug.LogError("ARCameraHandler: Phone Parent reference is missing!");
        }
    }
}