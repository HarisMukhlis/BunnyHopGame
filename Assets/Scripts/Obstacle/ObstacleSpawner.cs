using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject tallBranchPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject bigBranchPrefab;
    [SerializeField] private GameObject shortBranchPrefab;

    [Header("Spawner - General")]
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private Transform spawnParent;
    [Space]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private List<ObstaclePattern> patternObjectsPool;

    [SerializeField] private List<ObstaclePattern> _sortedPatternObjectsPool = new List<ObstaclePattern>();

    [Header("Spawn - Wood")]
    [SerializeField] private GameObject woodPrefab;
    [Space]
    [SerializeField] private Vector3 woodSpawnPos;
    [SerializeField] private Transform woodSpawnParent;
    [Space]
    [SerializeField] private Collider woodBaseCollider;

    [Header("Gap")]
    // [SerializeField] private float woodGapDistance = 1f;
    [SerializeField] private float woodGapChance = .5f;
    [SerializeField] private float woodGapChanceIncrement = .1f;
    [Space]
    [SerializeField] private float gapStartPos = 33f;
    [SerializeField] private float gapEndPos = 28f;
    [SerializeField] private float gapEndPosIncrement = 1f;
    [Space]
    [SerializeField] private float gapObstacleSpawnOffset = 1f;

    private bool _isSpawnable = true;
    private float _woodGapDistance;
    private WoodScript _currentWoodObj;
    private Vector3 _woodInitPos;
    private bool _isWoodMoving = false;
    private bool _isGapped = false;
    private float _initRotation = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        foreach (var pattern in patternObjectsPool)
        {
            var tempPattern = Instantiate(pattern);

            List<PatternObject> sortedObstacles = tempPattern.GetSortedList();
            tempPattern.obstacles = sortedObstacles;

            float prevOffset = 0f;
            foreach (var obstacle in tempPattern.obstacles)
            {
                float temp = obstacle.posOffset;

                obstacle.posOffset -= prevOffset;

                prevOffset = temp;
            }

            _sortedPatternObjectsPool.Add(tempPattern);
        }

        _woodGapDistance = gapStartPos - gapEndPos;
    }

    void Start()
    {
        _currentWoodObj = FindAnyObjectByType<WoodScript>();

        _woodInitPos = _currentWoodObj.gameObject.transform.localPosition;

        SpawnPattern();
    }

    #region Spawn Pattern

    [ContextMenu("[DEBUG]Spawn Random Obstacle")]
    private void SpawnPattern()
    {
        SpawnPattern(_sortedPatternObjectsPool[Random.Range(0, _sortedPatternObjectsPool.Count)], Random.Range(0f, 360f));
    }

    private void SpawnPattern(ObstaclePattern obstaclePattern, float rotation)
    {
        StartCoroutine(SpawnPatternRoutine(obstaclePattern, rotation));
    }

    private IEnumerator SpawnPatternRoutine(ObstaclePattern obstaclePattern, float rotation)
    {
        // _isSpawnable = false;
        Debug.Log("Pattern " + obstaclePattern.name + " has been spawned!");

        _initRotation = GameManager.Instance.sideOffset;

        List<PatternObject> patternObjects = obstaclePattern.obstacles;
        foreach (var obstacle in patternObjects)
        {
            float waitingDelay = obstacle.posOffset / GameManager.Instance.runningSpeed;
            yield return new WaitForSeconds(!float.IsNaN(waitingDelay) ? waitingDelay : 0f); //catch an undefined division

            if (obstacle.rotationRepeating < 1)
                SpawnObstacle(obstacle, rotation);
            else
            {
                for(int i = 0; i <= obstacle.rotationRepeating; i++)
                {
                    SpawnObstacle(obstacle, rotation, i);
                }
            }
        }

        yield return new WaitForSeconds(spawnInterval);

        if (!_isSpawnable)
            yield break;

        RollForGapSpawn();

        SpawnPattern();

        // _isSpawnable = true;
    }

    private void SpawnObstacle(PatternObject obstacle, float rotation, int index = 0)
    {
        float spawnRot = rotation + obstacle.GetRotationOffset(index) + (GameManager.Instance.sideOffset - _initRotation);
        Debug.Log("object " + obstacle +" at index : " + index + " | " + spawnRot + " " + obstacle.GetRotationOffset(index));

        if (!_isGapped)
        {
            switch (obstacle.objectType)
            {
                case PatternObject.ObjectType.TallBranch:
                    Instantiate(tallBranchPrefab, spawnPos, Quaternion.Euler(0f, 0f, spawnRot), spawnParent);
                    break;

                case PatternObject.ObjectType.Coin:
                    Instantiate(coinPrefab, spawnPos, Quaternion.Euler(0f, 0f, spawnRot), spawnParent);
                    break;

                case PatternObject.ObjectType.BigBranch:
                    Instantiate(bigBranchPrefab, spawnPos, Quaternion.Euler(0f, 0f, spawnRot), spawnParent);
                    break;

                case PatternObject.ObjectType.ShortBranch:
                    Instantiate(shortBranchPrefab, spawnPos, Quaternion.Euler(0f, 0f, spawnRot), spawnParent);
                    break;

                default:
                    break;
            }
        }
    }

    #endregion

    #region Spawn Gap

    [ContextMenu("[DEBUG]Spawn A Gap")]
    private void SpawnGap()
    {
        StartCoroutine(SpawnGapRoutine());
    }

    IEnumerator SpawnGapRoutine()
    {
        Debug.Log("Gap spawned");
        _currentWoodObj.MoveWood();
        _isWoodMoving = true;

        // yield return new WaitForSeconds(woodGapDistance);
        yield return new WaitUntil(() => _currentWoodObj.transform.localPosition.z <= _woodInitPos.z - _woodGapDistance);
        // Debug.Log("New wood spawned");

        GameObject spawnObj = Instantiate(woodPrefab, woodSpawnPos, Quaternion.identity, woodSpawnParent);
        WoodScript newWood = spawnObj.GetComponent<WoodScript>();
        newWood.MoveWood();

        //scuffed gap implementation, but kinda works
        yield return new WaitUntil(() => newWood.transform.localPosition.z <= gapStartPos + spawnPos.z + gapObstacleSpawnOffset);
        // Debug.Log("Gap enters spawnPos");
        _isGapped = true;

        yield return new WaitUntil(() => newWood.transform.localPosition.z <= gapEndPos + spawnPos.z - gapObstacleSpawnOffset);
        // Debug.Log("Gap exits spawnPos");
        _isGapped = false;

        yield return new WaitUntil(() => newWood.transform.localPosition.z <= gapStartPos);
        // Debug.Log("Gap started");
        StartGap();

        yield return new WaitUntil(() => newWood.transform.localPosition.z <= gapEndPos);
        // Debug.Log("Gap stopped");
        StopGap();

        yield return new WaitUntil(() => newWood.transform.localPosition.z <= _woodInitPos.z);
        // Debug.Log("New wood stopped");
        newWood.StopWood();
        _isWoodMoving = false;

        _currentWoodObj = newWood;
    }

    public void StartGap()
    {
        if (woodBaseCollider != null)
            woodBaseCollider.enabled = false;
    }

    public void StopGap()
    {
        if (woodBaseCollider != null)
            woodBaseCollider.enabled = true;
    }

    private void RollForGapSpawn()
    {
        if (!_isWoodMoving)
        {
            float roll = Random.Range(0f, 1f); //roll a chance for woodgap
            if (roll < woodGapChance)
            {
                SpawnGap();
                woodGapChance = .5f;
            }
            else woodGapChance += woodGapChanceIncrement;
        }
    }

    [ContextMenu("[DEBUG]Add Gap Distance")]
    public void AddGapDistance()
    {
        StartCoroutine(AddGapDistanceRoutine());
    }

    IEnumerator AddGapDistanceRoutine()
    {
        yield return new WaitUntil(() => !_isWoodMoving);

        gapStartPos += gapEndPosIncrement;
        _woodGapDistance = gapStartPos - gapEndPos;
    }

    #endregion

    public void StopSpawning()
    {
        _isSpawnable = false;
    }
}