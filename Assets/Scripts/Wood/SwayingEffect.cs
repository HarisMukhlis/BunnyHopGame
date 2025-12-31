using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SwayingEffect : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private List<GameObject> swayingObjects;

    [Header("Parameters")]
    [SerializeField] private float swayDuration = 4f;
    [SerializeField] private Vector3 swayStrength;
    [SerializeField] private int swayVibrato = 2;
    [SerializeField] private float swayRandomness = .4f;

    void Start()
    {
        InvokeRepeating("SwayObjects", 1f, swayDuration - 1f);
    }

    public void SwayObjects()
    {
        if (swayingObjects != null)
        {
            foreach (var obj in swayingObjects)
            {
                obj.transform.DOShakePosition(swayDuration, swayStrength, swayVibrato, swayRandomness);
            }
        }
    }
}