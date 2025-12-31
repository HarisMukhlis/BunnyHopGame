using System.Collections.Generic;
using DG.Tweening;
using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CamUIDebugger : MonoBehaviour
{
    [SerializeField] private List<Image> images;
    [SerializeField] private List<RawImage> rawImages;
    [SerializeField] private MultiHandLandmarkListAnnotation landmarkAnnotation;
    [Space]
    [SerializeField] private float opacity = .4f;
    [SerializeField] private float transitionDuration = 1f;

    private bool _isCamEnabled;

    private float _landmarkInitRadius;
    private float _landmarkInitLineWidth;

    void Start()
    {
        _landmarkInitRadius = landmarkAnnotation._landmarkRadius;
        _landmarkInitLineWidth = landmarkAnnotation._connectionWidth;

        DisableCam();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ToggleCam();
        }
    }

    private void ToggleCam()
    {
        if (_isCamEnabled) DisableCam();
        else EnableCam();

        landmarkAnnotation.RevalidateValues();
    }

    private void DisableCam()
    {
        _isCamEnabled = false;

        foreach (var img in images)
        {
            img.DOFade(0f, transitionDuration);
        }

        foreach(var rawImg in rawImages)
        {
            rawImg.DOFade(0f, transitionDuration);
        }

        landmarkAnnotation._landmarkRadius = 0f;
        landmarkAnnotation._connectionWidth = 0f;
    }

    private void EnableCam()
    {
        _isCamEnabled = true;

        foreach (var img in images)
        {
            img.DOFade(opacity, transitionDuration);
        }

        foreach(var rawImg in rawImages)
        {
            rawImg.DOFade(opacity, transitionDuration);
        }

        landmarkAnnotation._landmarkRadius = _landmarkInitRadius;
        landmarkAnnotation._connectionWidth = _landmarkInitLineWidth;
    }
}