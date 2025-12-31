using DG.Tweening;
using UnityEngine;

public class CoinScript : ObstacleScript
{
    // [SerializeField] private float minZPos = -5f;
    [Header("Coin Script")]
    [SerializeField] private float floatingFreq = 1f;
    [SerializeField] private float floatingAmp = 1f;
    // [SerializeField] private GameObject meshObject;

    [Space]
    [SerializeField] private string playerTag = "Player";

    private float _initTime;
    private TagHandle _playerTagHandle;

    private GameObject _camObject;

    override protected void Awake()
    {
        base.Awake();

        _initTime = Time.time;

        _playerTagHandle = TagHandle.GetExistingTag(playerTag);
    }

    override protected void Update()
    {
        base.MoveForward();
        base.SideRotation();

        MoveMesh();
    }

    override protected void SpawnSetup()
    {
        _camObject = GameManager.Instance.GetCamera().gameObject;
    }

    private void MoveMesh()
    {
        float freqProgress = (Time.time - _initTime) * floatingFreq - .5f;
        meshObject.transform.localPosition += Vector3.up * Mathf.Sin(freqProgress) * floatingAmp * Time.deltaTime;

        meshObject.transform.LookAt(_camObject.transform);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerTagHandle))
        {
            GameManager.Instance.AddScore();
            Destroy(this.gameObject);
        }
    }
}
