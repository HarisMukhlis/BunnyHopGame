using System;
using Unity.Mathematics;
using UnityEngine;

public class WoodScript : ObstacleScript
{
    // [SerializeField] private GameObject meshObject;
    [Header("Wood Script")]
    //remapping values from GameManager's RunningSpeed to the shader's ScrollingSpeed
    [SerializeField] private float remapIn = 5f;
    [SerializeField] private float remapOut = 0.647f;

    private bool _isMoving = false; //move when gap obstacle

    private MeshRenderer _renderer;
    private Material _woodMaterial;

    private int _scrollingSpeed;

    override protected void Awake()
    {
        try
        {
            _renderer = meshObject.GetComponent<MeshRenderer>();
            _woodMaterial = _renderer.material;

            _scrollingSpeed = Shader.PropertyToID("_ScrollingSpeed");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error at " + this.gameObject.name + " with code " + e);
        }
    }

    protected override void Update()
    {
        base.SideRotation();

        // SetScrollingShader();

        if (_isMoving)
            base.MoveForward();
    }

    override protected void SpawnSetup()
    {
        SetScrollingShader();
    }

    public void SetScrollingShader(float runningSpeed)
    {
        if (!_isMoving)
        {
            float scrollingValue = math.remap(0f, remapIn, 0f, remapOut, runningSpeed);
            _woodMaterial.SetFloat(_scrollingSpeed, scrollingValue);
        }
    }

    public void SetScrollingShader() //overflow
    {
        SetScrollingShader(GameManager.Instance.runningSpeed);
    }

    public void MoveWood()
    {
        SetScrollingShader(0f);

        _isMoving = true;
    }

    public void StopWood()
    {
        _isMoving = false;

        SetScrollingShader();
    }
}