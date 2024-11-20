using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ObjectUpdateHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "ObjectUpdate" };

    public void HandleData(string dataType, byte[] data)
    {
        Debug.Log($"ObjectUpdateHandler: Handling data of type {dataType}");
        if (dataType == "ObjectUpdate")
        {
            string jsonData = Encoding.UTF8.GetString(data);

            ObjectUpdateProvider.ObjectUpdate update = ObjectUpdateProvider.ObjectUpdate.FromJson(jsonData);
            int id = int.Parse(update.id);
            GameObject obj = GetObjectById(id);
            if (obj != null)
            {
                obj.transform.position = update.position;
            }
            else
            {
                Debug.LogError($"ObjectUpdateHandler: Object with id {update.id} not found");
            }
        }
        else
        {
            Debug.LogWarning($"ObjectUpdateHandler: Unsupported data type {dataType} received.");
        }
    }

    private GameObject GetObjectById(int id)
    {
        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj.GetInstanceID() == id)
            {
                return obj;
            }
        }
        return null;
    }
}
