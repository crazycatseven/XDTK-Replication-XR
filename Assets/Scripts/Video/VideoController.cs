using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private AudioSource audioSource;
    
    private void Awake()
    {
        // 获取组件
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 设置视频播放器
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        // 设置音频输出
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.controlledAudioTrackCount = 1;
        
        // 设置材质
        if (meshRenderer != null && renderTexture != null)
        {
            meshRenderer.material.mainTexture = renderTexture;
        }

        // 基本设置
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
    }

    public void SetVideoTime(float timeInSeconds)
    {
        if (videoPlayer != null)
        {
            videoPlayer.time = timeInSeconds;
            if (!videoPlayer.isPlaying)
                videoPlayer.Play();
        }
    }

    public void Play()
    {
        if (videoPlayer != null)
            videoPlayer.Play();
    }

    public void Pause()
    {
        if (videoPlayer != null)
            videoPlayer.Pause();
    }
} 