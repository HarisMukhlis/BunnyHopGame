using UnityEngine;

public static class MediaPipeFlag
{
    public static bool IsShuttingDown { get; private set; }

    public static void BeginShutdown()
    {
        IsShuttingDown = true;
    }
}
