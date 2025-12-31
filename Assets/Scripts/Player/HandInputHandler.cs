using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mediapipe;
// using Mediapipe.Tasks.Components.Containers;

public class HandInputHandler : MonoBehaviour
{
    [Header("Settings")]
    public float maxTiltAngle = 45f;
    [Range(0f, 1f)] public float jumpThreshold = 0.03f;

    [Header("Debug")]
    [SerializeField] private float currentSteering;
    [SerializeField] private bool isJumping;

    [Header("Outputs")]
    public UnityEvent<float> onSteer;
    public UnityEvent onJump;

    public void ProcessLandmarks(List<NormalizedLandmark> landmarks)
    {
        if (landmarks == null || landmarks.Count < 21) return;

        Debug.Log("Landmarks Processed! of " + landmarks + " count " + landmarks.Count);

        var wrist = landmarks[0]; //wrist point
        var middleMCP = landmarks[9]; //middle finger base point

        float dx = middleMCP.X - wrist.X;
        float dy = (1 - middleMCP.Y) - (1 - wrist.Y); 

        float angle = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
        
        float rawInput = Mathf.Clamp(angle / maxTiltAngle, -1f, 1f);
        
        if (Mathf.Abs(rawInput) < 0.15f) rawInput = 0f;

        currentSteering = rawInput; // invert if left/right is swapped
        onSteer.Invoke(currentSteering);
        
        var indexTIP = landmarks[8]; //index finger tip point
        var indexPIP = landmarks[6]; //index finger middle knuckle point(right above base)

        var middleTIP = landmarks[12]; // middle finger tip point
        var middlePIP = landmarks[10]; //then the middle knuckle

        bool indexCurled = indexTIP.Y > (indexPIP.Y + jumpThreshold);
        bool middleCurled = middleTIP.Y > (middlePIP.Y + jumpThreshold);

        Debug.Log($"Index : curled? {indexCurled} | Positions TIP : {indexTIP.Y} PIP : {indexPIP.Y}");
        Debug.Log($"Middle : curled? {middleCurled} | Positions TIP : {middleTIP.Y} PIP : {middlePIP.Y}");

        if (indexCurled && middleCurled)
        {
            if (!isJumping)
            {
                isJumping = true;
                onJump.Invoke();
            }
        }
        else
        {
            isJumping = false;
        }
    }

    //call when the landmarks arent detected
    public void ResetMovementInput()
    {
        currentSteering = 0f;
        onSteer.Invoke(currentSteering);

        isJumping = false;
    }

    // void Update()
    // {
    //     if (isJumping) onJump.Invoke();

    //     onSteer.Invoke(currentSteering);
    // }
}