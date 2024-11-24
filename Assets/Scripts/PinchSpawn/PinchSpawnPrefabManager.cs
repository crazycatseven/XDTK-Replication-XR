using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PinchSpawnPrefabMapping
{
    public string keyword;
    public GameObject prefab;
    public float initialScale = 0.5f;
}

public class PinchSpawnPrefabManager : MonoBehaviour
{
    [SerializeField]
    private List<PinchSpawnPrefabMapping> prefabMappings = new List<PinchSpawnPrefabMapping>();

    [SerializeField]
    private GameObject defaultPrefab;

    [SerializeField]
    private float defaultInitialScale = 0.3f;

    [SerializeField]
    private HandFollowSpawnHandler spawnHandler;

    private void Start()
    {
        if (spawnHandler == null)
        {
            spawnHandler = GetComponent<HandFollowSpawnHandler>();
        }

        if (spawnHandler != null)
        {
            spawnHandler.GetSpawnPrefab = GetPrefabByValue;
            Debug.Log("PinchSpawnPrefabManager: 已设置预制体获取委托");
        }
        else
        {
            Debug.LogError("PinchSpawnPrefabManager: 找不到 HandFollowSpawnHandler!");
        }
    }

    public (GameObject prefab, float scale) GetPrefabByValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return (defaultPrefab, defaultInitialScale);
        }

        foreach (var mapping in prefabMappings)
        {
            if (value.Contains(mapping.keyword, StringComparison.OrdinalIgnoreCase))
            {
                return (mapping.prefab, mapping.initialScale);
            }
        }

        return (defaultPrefab, defaultInitialScale);
    }

    public void AddPrefabMapping(string keyword, GameObject prefab)
    {
        prefabMappings.Add(new PinchSpawnPrefabMapping { keyword = keyword, prefab = prefab });
    }

    public void RemovePrefabMapping(string keyword)
    {
        prefabMappings.RemoveAll(mapping => mapping.keyword == keyword);
    }
}