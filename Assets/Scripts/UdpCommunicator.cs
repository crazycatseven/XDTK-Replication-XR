using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpCommunicator : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private IPEndPoint localEndPoint;
    private Thread receiveThread;

    public Action<string> OnMessageReceived; // 事件用于通知消息接收
    public int LocalPort { get; private set; }
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private bool isRunning = false;  // 控制线程的运行状态

    public void SetLocalPort(int port)
    {
        LocalPort = port;
        InitializeUdp();
    }

    // 初始化 UDP 客户端和端点
    private void InitializeUdp()
    {
        try
        {
            localEndPoint = new IPEndPoint(IPAddress.Any, LocalPort);
            udpClient = new UdpClient(localEndPoint);

            // 启动接收线程
            isRunning = true;
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("UDP Communicator started and listening on port " + LocalPort);
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing UDP: " + e.Message);
        }
    }

    // 设置远程 IP 和端口
    public void SetRemoteEndPoint(string ipAddress, int remotePort)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress parsedIP))
        {
            remoteEndPoint = new IPEndPoint(parsedIP, remotePort);
            Debug.Log("Remote endpoint set to " + ipAddress + ":" + remotePort);
        }
        else
        {
            Debug.LogError("Invalid IP Address");
        }
    }

    // 发送消息到远程端点
    public void SendUdpMessage(string message)
    {
        if (remoteEndPoint == null)
        {
            Debug.LogError("Remote endpoint is not set. Please set it before sending messages.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, remoteEndPoint);
            Debug.Log("Message sent via UDP: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    // 接收数据的线程函数，负责不断接收消息并加入队列
    private void ReceiveData()
    {
        try
        {
            // 设置接收超时时间（例如1000毫秒）
            udpClient.Client.ReceiveTimeout = 1000;

            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref localEndPoint); // 接收数据
                    string message = Encoding.UTF8.GetString(data);
                    messageQueue.Enqueue(message); // 将消息加入队列
                }
                catch (SocketException ex)
                {
                    // 这里捕获的是接收超时的异常
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        // 接收超时，继续循环检查 isRunning
                        continue;
                    }
                    else
                    {
                        // 处理其他 Socket 异常
                        Debug.LogError("SocketException: " + ex.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    // 主线程中处理队列中的消息
    void Update()
    {
        // 从队列中取出并处理消息，确保 Unity API 在主线程中调用
        while (messageQueue.TryDequeue(out string message))
        {
            OnMessageReceived?.Invoke(message); // 使用事件回调
        }
    }

    // 获取本地 IP 地址
    public string GetLocalIPAddress()
    {
        try
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting local IP address: " + e.Message);
            return "Unavailable";
        }
    }

    // 关闭 UDP 客户端和接收线程
    public void Close()
    {
        isRunning = false;  // 停止线程
        udpClient?.Close(); // 关闭UDP客户端

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(); // 等待线程自然退出
        }

        Debug.Log("UDP Communicator closed.");
    }

    private void OnApplicationQuit()
    {
        Close();
    }
}
