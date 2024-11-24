using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PinchSpawnDelegates
{
    public delegate (GameObject prefab, float scale) GetSpawnPrefabDelegate(string value);
    public delegate Vector3 GetSpawnPositionDelegate(string value);
    public delegate Vector3? GetSpawnStartPositionDelegate(string value);
}