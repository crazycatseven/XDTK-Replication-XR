using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using Oculus.Interaction;  // 为了OVRGrabbable
using Oculus.Interaction.HandGrab;

public class MeshToPinchSpawnPrefabConverter : EditorWindow
{
    private GameObject sourceMesh;
    private string savePath = "Assets/Prefabs/PinchSpawnPrefabs";

    [MenuItem("Tools/Convert To PinchSpawn Prefab")]
    public static void ShowWindow()
    {
        GetWindow<MeshToPinchSpawnPrefabConverter>("PinchSpawn Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh To PinchSpawn Prefab Converter", EditorStyles.boldLabel);
        sourceMesh = EditorGUILayout.ObjectField("Source Mesh", sourceMesh, typeof(GameObject), true) as GameObject;
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Convert Selected Mesh"))
        {
            if (sourceMesh == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a source mesh!", "OK");
                return;
            }
            ConvertToPinchSpawnPrefab(sourceMesh);
        }

        if (GUILayout.Button("Convert All Selected Objects"))
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select at least one object!", "OK");
                return;
            }

            foreach (GameObject obj in selectedObjects)
            {
                ConvertToPinchSpawnPrefab(obj);
            }
        }
    }

    private void ConvertToPinchSpawnPrefab(GameObject source)
    {
        // 创建根物体
        GameObject rootObject = new GameObject(source.name);
        rootObject.transform.position = source.transform.position;
        rootObject.transform.rotation = source.transform.rotation;

        // 添加Grabbable组件到根物体
        rootObject.AddComponent<Grabbable>();

        // 创建MeshObject
        GameObject meshObject = Instantiate(source, rootObject.transform);
        meshObject.name = "MeshObject";

        // 创建HandGrabInteractable物体（与MeshObject同级）
        GameObject handGrabObject = new GameObject("HandGrabPoint");
        handGrabObject.transform.SetParent(rootObject.transform);
        handGrabObject.transform.localPosition = Vector3.zero;
        handGrabObject.transform.localRotation = Quaternion.identity;

        // 配置HandGrabInteractable组件
        var handGrabInteractable = handGrabObject.AddComponent<HandGrabInteractable>();


        // 处理Collider
        Collider[] colliders = meshObject.GetComponentsInChildren<Collider>();
        Collider rootCollider = null;

        if (colliders.Length > 0)
        {
            // 使用第一个找到的collider作为模板
            Collider sourceCollider = colliders[0];

            // 复制collider到根物体
            if (sourceCollider is BoxCollider)
            {
                BoxCollider newCollider = rootObject.AddComponent<BoxCollider>();
                BoxCollider boxSource = (BoxCollider)sourceCollider;
                newCollider.center = boxSource.center;
                newCollider.size = boxSource.size;
                rootCollider = newCollider;
            }
            else if (sourceCollider is SphereCollider)
            {
                SphereCollider newCollider = rootObject.AddComponent<SphereCollider>();
                SphereCollider sphereSource = (SphereCollider)sourceCollider;
                newCollider.center = sphereSource.center;
                newCollider.radius = sphereSource.radius;
                rootCollider = newCollider;
            }
            else if (sourceCollider is CapsuleCollider)
            {
                CapsuleCollider newCollider = rootObject.AddComponent<CapsuleCollider>();
                CapsuleCollider capsuleSource = (CapsuleCollider)sourceCollider;
                newCollider.center = capsuleSource.center;
                newCollider.radius = capsuleSource.radius;
                newCollider.height = capsuleSource.height;
                newCollider.direction = capsuleSource.direction;
                rootCollider = newCollider;
            }
            else if (sourceCollider is MeshCollider)
            {
                MeshCollider newCollider = rootObject.AddComponent<MeshCollider>();
                MeshCollider meshSource = (MeshCollider)sourceCollider;
                newCollider.sharedMesh = meshSource.sharedMesh;
                newCollider.convex = true;
                rootCollider = newCollider;
            }

            // 删除原始的所有collider
            foreach (var collider in colliders)
            {
                DestroyImmediate(collider);
            }
        }
        else
        {
            // 如果没有找到collider，添加一个默认的BoxCollider
            BoxCollider boxCollider = rootObject.AddComponent<BoxCollider>();
            // 基于MeshRenderer的bounds设置大小
            var renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                boxCollider.center = renderer.bounds.center - rootObject.transform.position;
                boxCollider.size = renderer.bounds.size;
            }
            rootCollider = boxCollider;
        }

        // 设置Collider为trigger
        if (rootCollider != null)
        {
            rootCollider.isTrigger = true;
        }

        // 添加并配置Rigidbody
        Rigidbody rb = rootObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // 创建预制体
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string prefabPath = $"{savePath}/{rootObject.name}_PinchSpawn.prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
        DestroyImmediate(rootObject);

        Debug.Log($"Created PinchSpawn prefab at: {prefabPath}");
    }
}