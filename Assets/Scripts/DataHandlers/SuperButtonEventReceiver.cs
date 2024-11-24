using UnityEngine;
using UnityEngine.Events;
using System;

public class SuperButtonEventReceiver : MonoBehaviour
{

    [SerializeField]
    private string targetButtonId;

    // 定义事件数据结构
    [Serializable]
    public class DragEventData
    {
        public Vector2 totalDelta;
        public Vector2 currentDelta;
        public Vector2 position;
    }

    [Serializable]
    public class ReleaseEventData
    {
        public Vector2 totalDelta;
        public float duration;
        public Vector2 finalPosition;
    }

    // 定义UnityEvent
    public UnityEvent<string> onButtonClick;
    public UnityEvent<DragEventData> onButtonDrag;
    public UnityEvent<float> onButtonHold;
    public UnityEvent<ReleaseEventData> onButtonRelease;


    // 改为public，这样可以从外部调用
    public void HandleButtonEvent(string jsonData)
    {
        Debug.Log($"收到超级按钮事件: {jsonData}");

        // try
        // {
        //     var eventData = JsonUtility.FromJson<UIEventProvider.UIEventData>(jsonData);
        //     if (eventData == null)
        //     {
        //         Debug.LogError($"Failed to parse event data: {jsonData}");
        //         return;
        //     }

        //     switch (eventData.EventType)
        //     {
        //         case UIEventProvider.EventTypes.SuperButtonClick:
        //             HandleClick(eventData.Value);
        //             break;

        //         case UIEventProvider.EventTypes.SuperButtonDrag:
        //             var dragData = JsonUtility.FromJson<DragEventData>(eventData.Value);
        //             if (dragData != null) HandleDrag(dragData);
        //             break;

        //         case UIEventProvider.EventTypes.SuperButtonHold:
        //             if (float.TryParse(eventData.Value, out float holdDuration))
        //                 HandleHold(holdDuration);
        //             break;

        //         case UIEventProvider.EventTypes.SuperButtonRelease:
        //             var releaseData = JsonUtility.FromJson<ReleaseEventData>(eventData.Value);
        //             if (releaseData != null) HandleRelease(releaseData);
        //             break;
        //     }
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError($"Error handling button event: {e.Message}\nData: {jsonData}");
        // }
    }

    // 处理各种事件的虚方法，可以在子类中重写
    protected virtual void HandleClick(string value)
    {
        Debug.Log($"收到点击事件: {value}");
        onButtonClick?.Invoke(value);
    }

    protected virtual void HandleDrag(DragEventData dragData)
    {
        Debug.Log($"收到拖拽事件: 当前位置 = {dragData.position}, 拖拽距离 = {dragData.totalDelta}");
        onButtonDrag?.Invoke(dragData);
    }

    protected virtual void HandleHold(float duration)
    {
        Debug.Log($"收到长按事件: 持续时间 = {duration}秒");
        onButtonHold?.Invoke(duration);
    }

    protected virtual void HandleRelease(ReleaseEventData releaseData)
    {
        Debug.Log($"收到释放事件: 总移动距离 = {releaseData.totalDelta}, 持续时间 = {releaseData.duration}秒");
        onButtonRelease?.Invoke(releaseData);
    }

    // 获取当前监听的按钮ID
    public string GetTargetButtonId()
    {
        return targetButtonId;
    }
}