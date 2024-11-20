using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private bool isInitialized;
    private bool hasRemoteEndpoint;
    private readonly object lockObject = new object();

    private ConcurrentQueue<NetworkPacket> receiveQueue = new ConcurrentQueue<NetworkPacket>();

    private List<IDataProvider> dataProviders = new List<IDataProvider>();
    private List<IDataHandler> dataHandlers = new List<IDataHandler>();

    private Dictionary<string, List<IDataHandler>> handlerCache;

    [SerializeField] 
    private GameObject providersRoot;
    [SerializeField] 
    private GameObject handlersRoot;

    public IReadOnlyList<IDataProvider> DataProviders => dataProviders;
    public IReadOnlyList<IDataHandler> DataHandlers => dataHandlers;

    public struct NetworkPacket
    {
        public string DataType { get; set; }
        public byte[] Data { get; set; }
    }

    public bool IsConnected => isInitialized && hasRemoteEndpoint;
    public event Action<bool> OnConnectionStateChanged;

    private void NotifyConnectionStateChanged(bool connected)
    {
        OnConnectionStateChanged?.Invoke(connected);
        Debug.Log($"Network connection state changed: {connected}");
    }

    private void Awake()
    {
        if (providersRoot == null) providersRoot = transform.Find("DataProviders")?.gameObject;
        if (handlersRoot == null) handlersRoot = transform.Find("DataHandlers")?.gameObject;

        var localProviders = providersRoot.GetComponentsInChildren<IDataProvider>();
        var localHandlers = handlersRoot.GetComponentsInChildren<IDataHandler>();

        dataProviders.AddRange(localProviders);
        dataHandlers.AddRange(localHandlers);
    }

    private void Start()
    {
        CacheDataHandlers();
        RegisterDataProviders();
        StartCoroutine(ProcessQueue());
    }

    public void InitializeLocalListener(int localPort)
    {
        try
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
            }

            udpClient = new UdpClient(localPort);
            hasRemoteEndpoint = false;
            BeginReceive();
            isInitialized = true;
            NotifyConnectionStateChanged(IsConnected);
            Debug.Log($"Network initialized - Local Port: {localPort}, Remote: Not Set");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize network: {e.Message}");
            isInitialized = false;
            NotifyConnectionStateChanged(false);
        }
    }

    public void InitializeNetwork(string ip, int remotePort, int localPort)
    {
        try
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
            }

            udpClient = new UdpClient(localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), remotePort);
            hasRemoteEndpoint = true;
            BeginReceive();
            isInitialized = true;
            NotifyConnectionStateChanged(true);
            Debug.Log($"Network initialized - Local Port: {localPort}, Remote: {ip}:{remotePort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize network: {e.Message}");
            isInitialized = false;
            NotifyConnectionStateChanged(false);
        }
    }


    private void CacheDataHandlers()
    {
        handlerCache = new Dictionary<string, List<IDataHandler>>();

        foreach (var handler in dataHandlers)
        {
            foreach (var dataType in handler.SupportedDataTypes)
            {
                if (!handlerCache.ContainsKey(dataType))
                {
                    handlerCache[dataType] = new List<IDataHandler>();
                }

                handlerCache[dataType].Add(handler);
            }
        }

        Debug.Log("Handler cache initialized.");
    }

    private void RegisterDataProviders()
    {
        foreach (var provider in dataProviders)
        {
            provider.OnDataSend -= HandleDataSend;
            provider.OnDataSend += HandleDataSend;
            provider.Init(this);
        }
    }

    private void UnregisterDataProviders()
    {
        foreach (var provider in dataProviders)
        {
            provider.OnDataSend -= HandleDataSend;
        }
    }

    private void HandleDataSend(string dataType, byte[] data)
    {
        if (!isInitialized)
        {
            Debug.LogError($"Cannot send data: Network is not initialized");
            return;
        }

        if (!hasRemoteEndpoint)
        {
            Debug.LogError($"Cannot send data: Remote endpoint is not set");
            return;
        }

        if (string.IsNullOrEmpty(dataType))
        {
            Debug.LogError($"Cannot send data: Data type is null or empty");
            return;
        }

        if (data == null || data.Length == 0)
        {
            Debug.LogError($"Cannot send data: Data is null or empty");
            return;
        }

        SendData(dataType, data);
    }

    private void BeginReceive()
    {
        try
        {
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting receive: {e.Message}");
            isInitialized = false;
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(ar, ref remoteEP);

            if (data != null && data.Length > 0)
            {
                ProcessReceivedData(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in receive callback: {e.Message}");
        }
        finally
        {
            BeginReceive();
        }
    }

    private void ProcessReceivedData(byte[] data)
    {
        if (data == null || data.Length < 4)
        {
            Debug.LogError("Error processing received data: Data is null or too short.");
            return;
        }

        try
        {
            int typeLength = BitConverter.ToInt32(data, 0);

            if (typeLength < 0 || data.Length < 4 + typeLength)
            {
                Debug.LogError("Error processing received data: Data type length is invalid.");
                return;
            }

            string dataType = Encoding.UTF8.GetString(data, 4, typeLength);

            byte[] payload = new byte[data.Length - (4 + typeLength)];
            Array.Copy(data, 4 + typeLength, payload, 0, payload.Length);

            receiveQueue.Enqueue(new NetworkPacket
            {
                DataType = dataType,
                Data = payload
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing received data: {e.Message}");
        }
    }

    private IEnumerator ProcessQueue()
    {
        while (true)
        {
            while (receiveQueue.TryDequeue(out NetworkPacket packet))
            {
                if (handlerCache.TryGetValue(packet.DataType, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            handler.HandleData(packet.DataType, packet.Data);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error handling data type {packet.DataType}: {e.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No handler found for data type: {packet.DataType}");
                }
            }

            yield return null;
        }
    }

    public void SendData(string dataType, byte[] data)
    {
        if (hasRemoteEndpoint == false)
        {
            Debug.LogError("Error sending data: Remote endpoint is not set");
            return;
        }

        if (isInitialized == false)
        {
            Debug.LogError("Error sending data: Network is not initialized");
            return;
        }

        if (dataType == null || dataType == "")
        {
            Debug.LogError("Error sending data: Data type is null or empty");
            return;
        }

        if (data == null || data.Length == 0)
        {
            Debug.LogError("Error sending data: Data is null or empty");
            return;
        }

        try
        {
            byte[] typeBytes = Encoding.UTF8.GetBytes(dataType);
            byte[] packet = new byte[4 + typeBytes.Length + data.Length];

            BitConverter.GetBytes(typeBytes.Length).CopyTo(packet, 0);
            typeBytes.CopyTo(packet, 4);
            data.CopyTo(packet, 4 + typeBytes.Length);

            udpClient.Send(packet, packet.Length, remoteEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending data: {e.Message}");
        }
    }

    public void SetRemoteEndpoint(string ip, int port)
    {
        try
        {
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            hasRemoteEndpoint = true;
            NotifyConnectionStateChanged(IsConnected);
            Debug.Log($"Remote endpoint set to {ip}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set remote endpoint: {e.Message}");
            hasRemoteEndpoint = false;
            NotifyConnectionStateChanged(IsConnected);
        }
    }

    private void OnDestroy()
    {
        lock (lockObject)
        {
            UnregisterDataProviders();

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
                udpClient = null;

            }
        }
    }

    public void AddProvider(IDataProvider provider)
    {
        if (!dataProviders.Contains(provider))
        {
            dataProviders.Add(provider);
            provider.OnDataSend += HandleDataSend;
            provider.Init(this);
        }
    }

    public void AddHandler(IDataHandler handler)
    {
        if (!dataHandlers.Contains(handler))
        {
            dataHandlers.Add(handler);
            foreach (var dataType in handler.SupportedDataTypes)
            {
                if (!handlerCache.ContainsKey(dataType))
                {
                    handlerCache[dataType] = new List<IDataHandler>();
                }
                handlerCache[dataType].Add(handler);
            }
        }
    }
}