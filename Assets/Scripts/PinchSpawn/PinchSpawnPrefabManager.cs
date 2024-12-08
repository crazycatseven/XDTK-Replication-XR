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
    private HandFollowSpawnHandler handFollowSpawnHandler;

    [SerializeField]
    private SpreadSpawnHandler spreadSpawnHandler;

    private void Start()
    {
        if (handFollowSpawnHandler == null)
        {
            handFollowSpawnHandler = GetComponent<HandFollowSpawnHandler>();
        }

        if (handFollowSpawnHandler != null)
        {
            handFollowSpawnHandler.GetSpawnPrefab = GetPrefabByValue;
        }

        if (spreadSpawnHandler == null)
        {
            spreadSpawnHandler = GetComponent<SpreadSpawnHandler>();
        }

        if (spreadSpawnHandler != null)
        {
            spreadSpawnHandler.GetSpawnPrefab = GetPrefabByValue;
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