// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class HandCustomLandmarkerRunner : VisionTaskApiRunner<HandLandmarker>
    {
        public static HandCustomLandmarkerRunner Instance { get; private set; }
        [Space]
        [SerializeField] private HandLandmarkerResultAnnotationController _handLandmarkerResultAnnotationController;

        [Space]
        [Header("Events")]
        [SerializeField][Tooltip("Returns a -1 to 1 float, much like GetAxis")] private UnityEvent<List<Mediapipe.NormalizedLandmark>> onResultRun;
        [SerializeField] private UnityEvent onFailedRun;
        // public HandResultEvent onResultRun;

        private Experimental.TextureFramePool _textureFramePool;
        public readonly HandLandmarkDetectionConfig config = new HandLandmarkDetectionConfig();

        // Threading Sync Variables
        private HandLandmarkerResult _latestResult;
        private bool _hasNewResult;
        private readonly object _resultLock = new object();

        private bool _isShuttingDown;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        private void OnDisable()
        {
            Shutdown();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Shutdown()
        {
            if (_isShuttingDown) return;
            _isShuttingDown = true;

            StopAllCoroutines();
            Stop();
        }

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        void Update()
        {
            lock (_resultLock)
            {
                if (!_hasNewResult)
                {
                    return;
                }

                if (_latestResult.handLandmarks == null || _latestResult.handLandmarks.Count == 0)
                {
                    onFailedRun.Invoke();
                }
                else
                {
                    InvokeLandmarksData();
                }

                _hasNewResult = false;
            }
        }

        #region Send Landmarks
        private void InvokeLandmarksData(HandLandmarkerResult result)
        {
            if (result.handLandmarks != null && result.handLandmarks.Count > 0)
            {
                onResultRun.Invoke(ConvertToProtobuf(result.handLandmarks[0]));
            }
        }

        private void InvokeLandmarksData() //overflow
        {
            InvokeLandmarksData(_latestResult);
        }

        private List<Mediapipe.NormalizedLandmark> ConvertToProtobuf(Mediapipe.Tasks.Components.Containers.NormalizedLandmarks taskLandmarks)
        {
            var oldList = new List<Mediapipe.NormalizedLandmark>();

            if (taskLandmarks.landmarks != null)
            {
                foreach (var lm in taskLandmarks.landmarks)
                {
                    oldList.Add(new Mediapipe.NormalizedLandmark
                    {
                        X = lm.x,
                        Y = lm.y,
                        Z = lm.z
                    });
                }
            }
            return oldList;
        }

        #endregion

        #region Run Loop
        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Image Read Mode = {config.ImageReadMode}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumHands = {config.NumHands}");
            Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
            Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            //set up option. for the callback for LIVE_STREAM mode
            var options = config.GetHandLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnHandLandmarkDetectionOutput : null);
            taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            var imageSource = ImageSourceProvider.ImageSource;
            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);
            screen.Initialize(imageSource);
            SetupAnnotationController(_handLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();

            //mem allocations
            var result = HandLandmarkerResult.Alloc(options.numHands);

            var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                //image rendering
                Image image;
                switch (config.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage) throw new System.Exception("ImageReadMode.GPU is not supported");
                        textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildGPUImage(glContext);
                        yield return waitForEndOfFrame;
                        break;
                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;
                        textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        yield return waitUntilReqDone;
                        if (req.hasError) { continue; }
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                //hand detection
                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                            InvokeLandmarksData(result);
                        }
                        else
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(default);
                            onFailedRun.Invoke();
                        }
                        break;

                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(result);
                            InvokeLandmarksData(result);
                        }
                        else
                        {
                            _handLandmarkerResultAnnotationController.DrawNow(default);
                            onFailedRun.Invoke();
                        }
                        break;

                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        #endregion

        private void OnHandLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
        {
            _handLandmarkerResultAnnotationController.DrawLater(result);

            lock (_resultLock)
            {
                _latestResult = result;
                _hasNewResult = true;
            }
        }
    }
}