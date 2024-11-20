using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

public class SceneDataProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "SceneData";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    private Dictionary<string, GameObject> sceneObjectsMap = new Dictionary<string, GameObject>();

    [Serializable]
    public class ObjectData
    {
        public string id;
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string colliderType;
        public Vector3 colliderData;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ObjectData FromJson(string json)
        {
            return JsonUtility.FromJson<ObjectData>(json);
        }
    }


    [Serializable]
    public class SceneData
    {
        public List<ObjectData> objects = new List<ObjectData>();

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static SceneData FromJson(string json)
        {
            return JsonUtility.FromJson<SceneData>(json);
        }
    }


    public SceneData CollectSceneData()
    {
        SceneData sceneData = new SceneData();
        sceneObjectsMap.Clear();

        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj.TryGetComponent(out Collider collider))
            {
                var objectData = CreateObjectData(obj, collider);
                sceneData.objects.Add(objectData);
                sceneObjectsMap[objectData.id] = obj;
            }
        }

        return sceneData;
    }


    private ObjectData CreateObjectData(GameObject obj, Collider collider)
    {
        ObjectData data = new ObjectData
        {
            id = obj.GetInstanceID().ToString(),
            name = obj.name,
            position = obj.transform.position,
            scale = obj.transform.localScale
        };

        if (collider is BoxCollider boxCollider)
        {
            data.colliderType = "Box";
            data.colliderData = boxCollider.size;
        }
        else if (collider is SphereCollider sphereCollider)
        {
            data.colliderType = "Sphere";
            data.colliderData = new Vector3(sphereCollider.radius, 0, 0);
        }

        return data;
    }

    public void SendSceneData()
    {
        if (!IsEnabled) return;
        
        SceneData sceneData = CollectSceneData();
        string dataJson = sceneData.ToJson();
        byte[] data = Encoding.UTF8.GetBytes(dataJson);
        OnDataSend?.Invoke(DataType, data);
    }

    [ContextMenu("Debug Scene Data")]
    private void DebugSendSceneData()
    {
        SendSceneData();
    }

}