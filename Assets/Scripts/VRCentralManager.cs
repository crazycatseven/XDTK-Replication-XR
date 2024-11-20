using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCentralManager : MonoBehaviour
{

    public NetworkManager networkManager;
    public bool useRemoteEndpoint = false;
    [SerializeField]
    private string ipAddress;
    [SerializeField] 
    private int remotePort;
    public int localPort = 9982;

    public string IpAddress 
    {
        get => useRemoteEndpoint ? ipAddress : null;
        set => ipAddress = value;
    }

    public int RemotePort
    {
        get => useRemoteEndpoint ? remotePort : 0;
        set => remotePort = value;
    }


    // Start is called before the first frame update
    void Start()
    {
        networkManager = networkManager ?? GetComponent<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager component not found!");
        }

        if (useRemoteEndpoint)
        {
            networkManager.InitializeNetwork(ipAddress, remotePort, localPort);
        }
        else
        {
            networkManager.InitializeLocalListener(localPort);
        }

    }
}
