using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using DG.Tweening;

[System.Serializable]
public class ObjectRecycleEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandGrabInteractable grabInteractable;
    [SerializeField] private AudioClip recycleSound;

    private Transform _wristOffsetTransform;
    private Transform meshObject;
    private Collider[] objectColliders;
    private AudioSource audioSource;

    [Header("Recycle Settings")]
    [SerializeField] private float recycleDistance = 0.1f;    // 触发回收的距离
    [SerializeField] private float recycleDuration = 0.5f;    // 回收动画总时长
    [SerializeField] private float xzScaleMultiplier = 2.0f;  // XZ轴缩放速度倍率
    [SerializeField] private float yScaleMultiplier = 0.8f;   // Y轴缩放速度倍率
    [SerializeField] private float yMoveOffset = 0.05f;   // Y轴移动速度倍率
    [SerializeField] private float destroyDelay = 0.5f;       // 销毁延迟时间

    private bool isRecycling = false;
    private Vector3 recycleStartPosition;
    private Vector3 recycleStartScale;
    private float recycleStartTime;
    private Vector3 velocity = Vector3.zero;
    private bool canBeRecycled = false;  // 新增：标记是否可以被回收

    private void Start()
    {
        // 获取组件引用
        if (grabInteractable == null)
            grabInteractable = GetComponentInChildren<HandGrabInteractable>();

        meshObject = transform.Find("MeshObject");
        objectColliders = GetComponentsInChildren<Collider>();
        audioSource = gameObject.AddComponent<AudioSource>();

        // 查找手部参考点
        _wristOffsetTransform = GameObject.Find("HandWristOffsetPointLeft")?.transform;

        // 注册抓取事件
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorRemoved.Action += OnReleased;
        }
    }

    private void OnReleased(IHandGrabInteractor interactor)
    {
        if (!isRecycling && ShouldStartRecycle())
        {
            StartRecycle();
        }
    }

    private bool ShouldStartRecycle()
    {
        if (_wristOffsetTransform == null) return false;
        if (!canBeRecycled) return false;

        // 计算手掌中心点
        Vector3 handCenter = _wristOffsetTransform.position;

        // 检查抓取点与手掌中心的距离
        float distance = Vector3.Distance(grabInteractable.transform.position, handCenter);
        return distance <= recycleDistance;
    }

    private void StartRecycle()
    {
        if (this == null || !gameObject.activeInHierarchy) return;

        isRecycling = true;
        recycleStartTime = Time.time;
        recycleStartPosition = transform.position;
        recycleStartScale = meshObject.localScale;

        // 禁用碰撞体
        foreach (var collider in objectColliders)
        {
            if (collider != null)
                collider.enabled = false;
        }

        // 禁用抓取交互
        if (grabInteractable != null)
            grabInteractable.enabled = false;

        // 播放音效
        if (recycleSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(recycleSound);
        }

        // 立即开始动画
        if (_wristOffsetTransform != null)
        {
            Vector3 handCenter = _wristOffsetTransform.position;
            
            Sequence sequence = DOTween.Sequence();

            // 在序列完成时清理
            sequence.OnComplete(() => {
                if (this != null && gameObject != null)
                {
                    // 在销毁前禁用组件而不是直接销毁
                    enabled = false;
                    StartCoroutine(DelayedDestroy());
                }
            });

            // 创建位移动画
            sequence.Join(transform.DOMove(handCenter, recycleDuration)
                .SetEase(Ease.InBack));

            // 创建缩放动画
            sequence.Join(meshObject.DOScale(Vector3.zero, recycleDuration)
                .SetEase(Ease.InFlash));

            // 添加局部Y轴下移动画
            sequence.Join(meshObject.DOLocalMoveY(-yMoveOffset, recycleDuration)
                .SetEase(Ease.InBack));

            sequence.SetAutoKill(true).Play();
        }
    }

    private IEnumerator DelayedDestroy()
    {
        // 确保所有动画完成
        yield return new WaitForSeconds(recycleDuration);
        
        // 等待额外延迟
        yield return new WaitForSeconds(destroyDelay);
        
        // 在销毁前先禁用所有组件
        if (this != null && gameObject != null)
        {
            // 禁用所有组件
            var components = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component != null)
                    component.enabled = false;
            }
            
            // 延迟一帧后再销毁
            yield return null;
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        // 确保清理所有动画
        DOTween.Kill(transform);
        DOTween.Kill(meshObject);
    }

    private void OnDestroy()
    {
        // 确保清理所有动画和事件
        DOTween.Kill(transform);
        DOTween.Kill(meshObject);
        
        // 清理其他引用
        _wristOffsetTransform = null;
        meshObject = null;
        grabInteractable = null;

        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorRemoved.Action -= OnReleased;
        }
    }

    private void Update()
    {
        if (!isRecycling && grabInteractable != null && 
            grabInteractable.State == InteractableState.Select)
        {
            float distance = Vector3.Distance(transform.position, _wristOffsetTransform.position);
            
            // 如果还不能回收，检查是否已经离开过回收范围
            if (!canBeRecycled)
            {
                if (distance > recycleDistance)
                {
                    canBeRecycled = true;  // 一旦离开范围，标记为可回收
                }
            }
            // 只有在可回收状态下才检查是否应该开始回收
            else if (distance <= recycleDistance)
            {
                StartRecycle();
            }
        }
    }
}