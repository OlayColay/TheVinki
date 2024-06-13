using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Vinki;

public class FullscreenVideo(ProcessManager manager) : MainLoopProcess(manager, Enums.FullscreenVideo)
{
    public ProcessManager.ProcessID nextProcess;
    public VideoPlayer videoPlayer;

    public void StartVideo(string videoFileName, ProcessManager.ProcessID nextProcess)
    {
        this.nextProcess = nextProcess;

        // Create a Canvas GameObject
        GameObject gameObject = new(nameof(FullscreenVideo));
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create a RawImage
        RawImage rawImage = gameObject.AddComponent<RawImage>();
        rawImage.rectTransform.anchoredPosition = Vector2.zero;
        rawImage.rectTransform.sizeDelta = new Vector2(0, 0);
        rawImage.rectTransform.anchorMin = new Vector2(0, 0);
        rawImage.rectTransform.anchorMax = new Vector2(1, 1);
        rawImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Create a RenderTexture
        RenderTexture renderTexture = new(Screen.width, Screen.height, 24);
        rawImage.texture = renderTexture;

        // Create a VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.url = AssetManager.ResolveFilePath(videoFileName);
        videoPlayer.isLooping = false;

        // Play the video
        RWCustom.Custom.Log("Preparing video...");
        videoPlayer.prepareCompleted += PrepareCompleted;
        videoPlayer.Prepare();
    }

    private void PrepareCompleted(VideoPlayer source)
    {
        RWCustom.Custom.Log("Playing video of length " + source.length.ToString());
        source.Play();
        source.loopPointReached += VideoFinished;
    }

    private void VideoFinished(VideoPlayer source)
    {
        RWCustom.Custom.Log("Video finished!");
        manager.RequestMainProcessSwitch(nextProcess);
        UnityEngine.Object.Destroy(source.gameObject);
    }

    public override void Update()
    {
        base.Update();

        if (RWInput.CheckPauseButton(0))
        {
            videoPlayer.Stop();
            VideoFinished(videoPlayer);
        }
    }
}
