using System.Collections;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
  public class Bootstrap : MonoBehaviour
  {
    [SerializeField] private AppSettings _appSettings;

    public InferenceMode inferenceMode { get; private set; }
    public bool isFinished { get; private set; }

    private static bool _initialized;
    private static bool _isGlogInitialized;

    private void Awake()
    {
      if (_initialized)
      {
        Destroy(gameObject);
        return;
      }

      DontDestroyOnLoad(gameObject);
      _initialized = true;
    }

    private void OnEnable()
    {
      if (_isGlogInitialized || MediaPipeFlag.IsShuttingDown)
      {
        return;
      }

      StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
      Debug.Log("The configuration for the sample app can be modified using AppSettings.asset.");

      Logger.MinLogLevel = _appSettings.logLevel;
      Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);

      Debug.Log("Setting global flags...");
      _appSettings.ResetGlogFlags();
      Glog.Initialize("MediaPipeUnityPlugin");
      _isGlogInitialized = true;

      Debug.Log("Initializing AssetLoader...");
      switch (_appSettings.assetLoaderType)
      {
        case AppSettings.AssetLoaderType.AssetBundle:
          AssetLoader.Provide(new AssetBundleResourceManager("mediapipe"));
          break;

        case AppSettings.AssetLoaderType.StreamingAssets:
          AssetLoader.Provide(new StreamingAssetsResourceManager());
          break;

#if UNITY_EDITOR
        case AppSettings.AssetLoaderType.Local:
          AssetLoader.Provide(new LocalResourceManager());
          break;
#endif
        default:
          Debug.LogError($"AssetLoaderType is unknown: {_appSettings.assetLoaderType}");
          yield break;
      }

      DecideInferenceMode();

      if (inferenceMode == InferenceMode.GPU)
      {
        yield return GpuManager.Initialize();
      }

      ImageSourceProvider.Initialize(
        _appSettings.BuildWebCamSource(),
        _appSettings.BuildStaticImageSource(),
        _appSettings.BuildVideoSource()
      );

      ImageSourceProvider.Switch(_appSettings.defaultImageSource);

      isFinished = true;
    }

    private void DecideInferenceMode()
    {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
      inferenceMode = InferenceMode.CPU;
#else
      inferenceMode = _appSettings.preferableInferenceMode;
#endif
    }

    private void OnApplicationQuit()
    {
      MediaPipeFlag.BeginShutdown();

      GpuManager.Shutdown();

      if (_isGlogInitialized)
      {
        Glog.Shutdown();
      }

      Protobuf.ResetLogHandler();
    }
  }
}
