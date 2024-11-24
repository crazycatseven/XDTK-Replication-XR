using UnityEngine;

public class GestureValueProvider : MonoBehaviour
{
    [SerializeField] private ScreenGestureProvider gestureProvider;

    private void Start()
    {
        if (gestureProvider == null)
        {
            gestureProvider = GetComponent<ScreenGestureProvider>();
        }

        if (gestureProvider != null)
        {
            gestureProvider.GetGestureValue = GetCurrentGestureValue;
        }
    }

    private string GetCurrentGestureValue()
    {
        // 这里可以根据不同条件返回不同的value
        // 例如：检查当前UI状态、选中的产品等

        // 示例实现：
        if (IsInProductPage1())
        {
            return "Product1";
        }
        else if (IsInProductPage2())
        {
            return "Product2";
        }

        return string.Empty;
    }

    // 示例方法：检查是否在产品1页面
    private bool IsInProductPage1()
    {
        // 实现您的检查逻辑
        return false;
    }

    // 示例方法：检查是否在产品2页面
    private bool IsInProductPage2()
    {
        // 实现您的检查逻辑
        return false;
    }

    // 公共方法：允许其他脚本更新当前状态
    public void SetCurrentPage(string pageName)
    {
        // 如果需要，可以添加状态管理逻辑
    }
}