using System;
using UnityEngine;

public interface IDataProvider
{
    string DataType { get; }
    bool IsEnabled { get; set; }
    event Action<string, byte[]> OnDataSend;
    void Init(NetworkManager networkManager)
    {
        if (this is MonoBehaviour provider)
        {
            provider.StartCoroutine(InitializeProvider(networkManager));
        }
    }

    System.Collections.IEnumerator InitializeProvider(NetworkManager networkManager)
    {
        yield return new WaitForEndOfFrame();

        if (networkManager != null)
        {
            networkManager.OnConnectionStateChanged += HandleConnectionStateChanged;
            HandleConnectionStateChanged(networkManager.IsConnected);
            Debug.Log($"Provider {DataType} initialized with NetworkManager");
        }
        else
        {
            Debug.LogError($"Provider {DataType} failed to initialize: NetworkManager is null");
        }
    }

    void HandleConnectionStateChanged(bool connected)
    {
        IsEnabled = connected;
    }
}