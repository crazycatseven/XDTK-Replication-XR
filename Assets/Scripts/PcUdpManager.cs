using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using System.Net.NetworkInformation;

public class PcUdpManager : MonoBehaviour
{


    public GameObject currentSelectedObject;
    public GameObject phoneParent;
    public GameObject phone;
    public int localPort = 9981;
    private UdpCommunicator udpCommunicator;

    private Quaternion rotationOffset = Quaternion.identity;

    private Vector3 positionOffset = new Vector3(0, 0, 0);

    private Quaternion receivedRotation = Quaternion.identity;

    private Vector3 receivedPosition = new Vector3(0, 0, 0);


    public OVRSkeleton skeleton;
    public Transform index1;
    public Transform index2;
    public Transform index3;
    public Transform indexTip;

    public Transform thumb0;
    public Transform thumb1;
    public Transform thumb2;
    public Transform thumb3;
    public Transform thumbTip;

    public LineRenderer rayLineRenderer;

    public RayCollisionDetector rayCollisionDetector;

    public LineRenderer lineRendererForward;
    public LineRenderer lineRendererUp;
    public LineRenderer lineRendererRight;


    void Start()
    {
        // 初始化 UdpCommunicator
        udpCommunicator = GetComponent<UdpCommunicator>();
        udpCommunicator.SetLocalPort(localPort);
        udpCommunicator.OnMessageReceived = OnMessageReceived;  // 设置回调函数

        lineRendererForward.startWidth = 0.01f;
        lineRendererForward.endWidth = 0.01f;
        lineRendererUp.startWidth = 0.01f;
        lineRendererUp.endWidth = 0.01f;
        lineRendererRight.startWidth = 0.01f;
        lineRendererRight.endWidth = 0.01f;

        lineRendererForward.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererUp.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererRight.material = new Material(Shader.Find("Sprites/Default"));

        lineRendererForward.startColor = Color.blue;
        lineRendererForward.endColor = Color.blue;
        lineRendererUp.startColor = Color.green;
        lineRendererUp.endColor = Color.green;
        lineRendererRight.startColor = Color.red;
        lineRendererRight.endColor = Color.red;
    }

    void Update(){

        rayLineRenderer.SetPosition(0, phone.transform.position);
        rayLineRenderer.SetPosition(1, phone.transform.position + phone.transform.up * 5);

        lineRendererForward.SetPosition(0, phone.transform.position);
        lineRendererForward.SetPosition(1, phone.transform.position + phone.transform.forward * 0.2f);
        
        lineRendererUp.SetPosition(0, phone.transform.position);
        lineRendererUp.SetPosition(1, phone.transform.position + phone.transform.up * 0.2f);

        lineRendererRight.SetPosition(0, phone.transform.position);
        lineRendererRight.SetPosition(1, phone.transform.position + phone.transform.right * 0.2f);
    }

    private void OnMessageReceived(string message)
    {
        try
        {
            // 打印接收到的完整消息，帮助调试
            Debug.Log("Received message: " + message);

            string[] parts = message.Split('|');
            if (parts.Length < 2)
            {
                Debug.LogWarning("Invalid message format: " + message);
                return;
            }

            string messageType = parts[0];
            string payload = parts[1];

            Debug.Log("Received message type: " + messageType);

            switch (messageType)
            {
                case "ArCameraData":
                    // 处理ArCameraData类型消息
                    string[] positionAndRotation = payload.Split(new string[] { "Position:", "Rotation:" }, StringSplitOptions.RemoveEmptyEntries);

                    if (positionAndRotation.Length == 2)
                    {
                        // 移除空格并解析位置和旋转值
                        string[] positionData = positionAndRotation[0].Trim().Split(',');
                        string[] rotationData = positionAndRotation[1].Trim().Split(',');

                        if (positionData.Length == 3 && rotationData.Length == 4)
                        {
                            receivedPosition = new Vector3(
                                float.Parse(positionData[0]),
                                float.Parse(positionData[1]),
                                float.Parse(positionData[2])
                            );

                            // 解析并更新物体的位置和旋转
                            

                            float x = float.Parse(rotationData[0]);
                            float y = float.Parse(rotationData[1]);
                            float z = float.Parse(rotationData[2]);  
                            float w = float.Parse(rotationData[3]);

                            receivedRotation = new Quaternion(x, y, z, w);
                            receivedRotation.Normalize();

                            phoneParent.transform.rotation = rotationOffset * receivedRotation;

                            phoneParent.transform.position = receivedPosition + positionOffset;

                        }
                        else
                        {
                            Debug.LogWarning("Invalid position or rotation data format: " + payload);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid payload format for ArCameraData: " + payload);
                    }
                    break;
                
                case "ButtonPressed":
                    
                    if (payload.Contains("Align")){
                        gestureAlign();
                    }
                    else if (payload.Contains("Select")){
                        if (rayCollisionDetector.GetCurrentHitObject() != null){
                            // 如果之前有选中的物体，恢复其颜色
                            if (currentSelectedObject != null){
                                currentSelectedObject.GetComponent<MeshRenderer>().material.color = Color.white;
                            }
                            // 选中新物体并改变颜色
                            currentSelectedObject = rayCollisionDetector.GetCurrentHitObject();
                            currentSelectedObject.GetComponent<MeshRenderer>().material.color = Color.red;
                            Debug.Log("Selected object: " + currentSelectedObject.name);
                        } else {
                            // 如果没有选中新物体，且之前有选中的物体，恢复其颜色
                            if (currentSelectedObject != null){
                                currentSelectedObject.GetComponent<MeshRenderer>().material.color = Color.white; // 假设默认颜色是白色
                                currentSelectedObject = null;
                            }
                        }
                    }
                    break;

                case "JoystickData":
                    string[] joystickData = payload.Split(',');
                    if (joystickData.Length == 3)
                    {
                        string description = joystickData[0];
                        float horizontal = float.Parse(joystickData[1]) * 0.01f;
                        float vertical = float.Parse(joystickData[2]) * 0.01f;
                        // 处理JoystickData类型消息

                        if (description == "Move"){
                            currentSelectedObject.transform.position += new Vector3(horizontal, vertical, 0);
                        }
                        else if (description == "Rotate"){
                            currentSelectedObject.transform.rotation = receivedRotation * Quaternion.Euler(0, horizontal, 0) * Quaternion.Euler(vertical, 0, 0);
                        }
                    }
                    break;

                default:
                    Debug.LogWarning("Unknown message type: " + messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing message: " + ex.Message);
        }
    }


    private string GetLocalIPAddress()
    {
        try
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
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

    public void gestureAlign(){

        phoneParent.transform.rotation = receivedRotation;
        phoneParent.transform.position = receivedPosition;

        Quaternion rotationA = Quaternion.LookRotation(phoneParent.transform.forward, phoneParent.transform.up);

        Vector3 upB = (indexTip.position - index1.position).normalized;
        Vector3 rightB = (thumbTip.position - thumb1.position).normalized;
        Vector3 forwardB = Vector3.Cross(rightB, upB).normalized;

        Quaternion rotationB = Quaternion.LookRotation(forwardB, upB);

        rotationOffset = rotationB * Quaternion.Inverse(rotationA);
        // 计算屏幕中间位置 = 拇指根部位置 + 拇指根部位置到拇指尖位置的向量 * 手机宽度的一半 + upB * 手机高度的一半
        Vector3 screenCenter = thumb1.position + rightB * phone.transform.localScale.x / 2 + upB * phone.transform.localScale.y;

        // Vector3 screenCenter = thumb0.position;

        // 计算位置偏移 = 手机位置 - 屏幕中间位置
        positionOffset =  screenCenter - phoneParent.transform.position;
    }

    void OnApplicationQuit()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.Close();
        }
    }


}
