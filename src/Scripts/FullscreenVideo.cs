using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Vinki;

public class FullscreenVideo(ProcessManager manager) : MainLoopProcess(manager, Enums.FullscreenVideo)
{
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private RenderTexture renderTexture;
    private ProcessManager.ProcessID nextProcess;

    public void StartVideo(string videoFileName, ProcessManager.ProcessID nextProcess)
    {
        this.nextProcess = nextProcess;

        this.manager.RequestMainProcessSwitch(Enums.FullscreenVideo);

        // Create a Canvas GameObject
        GameObject canvasGameObject = new("Canvas");
        Canvas canvas = canvasGameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create a RawImage GameObject
        GameObject rawImageGameObject = new("RawImage");
        rawImageGameObject.transform.SetParent(canvasGameObject.transform);
        rawImage = rawImageGameObject.AddComponent<RawImage>();
        rawImage.rectTransform.anchoredPosition = Vector2.zero;
        rawImage.rectTransform.sizeDelta = new Vector2(0, 0);
        rawImage.rectTransform.anchorMin = new Vector2(0, 0);
        rawImage.rectTransform.anchorMax = new Vector2(1, 1);
        rawImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Create a RenderTexture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        rawImage.texture = renderTexture;

        // Create a VideoPlayer GameObject
        GameObject videoPlayerGameObject = new("VideoPlayer");
        videoPlayer = videoPlayerGameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.url = AssetManager.ResolveFilePath(videoFileName);
        videoPlayer.isLooping = false;

        // Play the video
        videoPlayer.prepareCompleted += PrepareCompleted;
        videoPlayer.Prepare();
    }

    private void PrepareCompleted(VideoPlayer source)
    {
        videoPlayer.Play();
        videoPlayer.loopPointReached += VideoFinished;
    }

    private void VideoFinished(VideoPlayer source)
    {
        manager.RequestMainProcessSwitch(nextProcess);
    }
}
