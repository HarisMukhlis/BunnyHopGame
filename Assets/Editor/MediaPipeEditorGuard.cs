#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class MediaPipeEditorGuard
{
    static MediaPipeEditorGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            MediaPipeFlag.BeginShutdown();
        }
    }
}
#endif
