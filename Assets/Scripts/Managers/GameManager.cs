using System.Collections;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private Camera mainCamera;

    [Header("Difficulty Adjustments")]
    [SerializeField] private int difficultyScoreIncrement = 2;
    [SerializeField] private float runningSpeedIncrement = .5f;
    [SerializeField] private float rotationSpeedIncrement = 1f;

    [Header("Events")]
    [SerializeField] private UnityEvent onGameOver;
    [SerializeField] private UnityEvent onSpeedChange;

    public int score { get; private set; } = 0;
    public float sideOffset = 0f;
    public float runningSpeed = 5f;
    public float rotationSpeed = 15f;

    private int _nextScoreDifficulty = 5;

    private bool _isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this.gameObject);
    }

    void Update()
    {
        sideOffset += playerScript.sideMovement * Time.deltaTime * rotationSpeed;
        // Debug.Log("TEST" + playerScript.sideMovement * Time.deltaTime + " " + sideOffset);
    }

    public float GetSideMovement()
    {
        if (playerScript != null)
            return playerScript.sideMovement;
        else return 0f;
    }

    public void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        Debug.Log("Game Over!");

        runningSpeed = 0f;

        GameUIManager.Instance.ShowGameOverScreen();
        onGameOver.Invoke();
    }

    public void AddScore()
    {
        score++;

        GameUIManager.Instance.UpdateScore();

        if (score > _nextScoreDifficulty)
        {
            runningSpeed += runningSpeedIncrement;
            _nextScoreDifficulty += _nextScoreDifficulty + difficultyScoreIncrement;
            rotationSpeed += rotationSpeedIncrement;

            onSpeedChange.Invoke();
        }
    }

    public Camera GetCamera()
    {
        return mainCamera;
    }

    public void RestartGame()
    {
        StartCoroutine(RestartGameRoutine());
    }

    IEnumerator RestartGameRoutine()
    {
        if (HandCustomLandmarkerRunner.Instance != null)
        {
            HandCustomLandmarkerRunner.Instance.enabled = false;
            yield return null;
            yield return null; //two frame wait, for OnDisable to work
        }

        StaticManager.Instance.RestartGameScene();
    }

    public void GoToMainMenu()
    {
        StartCoroutine(GoToMainMenuRoutine());
    }

    IEnumerator GoToMainMenuRoutine()
    {
        if (HandCustomLandmarkerRunner.Instance != null)
        {
            HandCustomLandmarkerRunner.Instance.enabled = false;
            yield return null;
            yield return null; //two frame wait, for OnDisable to work
        }

        StaticManager.Instance.LoadScene("MainMenuScene");
    }
}
