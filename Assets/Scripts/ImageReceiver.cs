using System.Collections.Generic;
using UnityEngine;

public class ImageReceiver
{
    private Dictionary<int, byte[]> receivedPackets = new Dictionary<int, byte[]>();
    private int expectedPackets = -1;
    private int packetSize = 1024;  // 假设每个包1KB

    public event System.Action<Texture2D> OnImageReceived;  // 图片接收完毕事件

    public void ReceivePacket(byte[] data)
    {
        // 解析包序号和总包数
        int packetIndex = System.BitConverter.ToInt32(data, 0);
        int totalPackets = System.BitConverter.ToInt32(data, 4);

        // 更新总包数
        if (expectedPackets == -1) expectedPackets = totalPackets;

        // 保存当前包的数据
        byte[] imageData = new byte[data.Length - 8];
        System.Array.Copy(data, 8, imageData, 0, imageData.Length);
        receivedPackets[packetIndex] = imageData;

        // 检查是否已接收所有数据包
        if (receivedPackets.Count == expectedPackets)
        {
            List<byte> completeImageData = new List<byte>();
            for (int i = 0; i < expectedPackets; i++) completeImageData.AddRange(receivedPackets[i]);

            // 加载图片
            Texture2D receivedImage = new Texture2D(2, 2);
            receivedImage.LoadImage(completeImageData.ToArray());

            // 触发图片接收完毕事件
            OnImageReceived?.Invoke(receivedImage);
            receivedPackets.Clear();  // 重置状态
            expectedPackets = -1;
        }
    }
}
