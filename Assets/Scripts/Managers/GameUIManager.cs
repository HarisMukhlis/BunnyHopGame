using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Space]
    [SerializeField] private RectTransform gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Parameters")]
    [SerializeField] private float gameOverTransitionDuration = 1f;
    [SerializeField] private float gameOverPanelFade = .6f;
    [Space]
    [SerializeField] private float scoreJumpHeight = 10f;
    [SerializeField] private float scoreJumpDuration = 1f;

    private int _finalScoreDisplayNumber = 0;
    private int finalScoreDisplayNumber
    {
        get => _finalScoreDisplayNumber;
        set
        {
            _finalScoreDisplayNumber = value;
            finalScoreText.text = _finalScoreDisplayNumber.ToString();
        }
    }

    private Tween _scoreTween;
    private Vector2 _initScorePos;
    private RectTransform _scoreTransform;

    private Tween _finalScoreTween;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this.gameObject);
    }

    void Start()
    {
        if (gameOverPanel.gameObject.activeSelf)
            gameOverPanel.gameObject.SetActive(false);
        
        _scoreTransform = scoreText.gameObject.GetComponent<RectTransform>();

        _initScorePos = _scoreTransform.anchoredPosition;
    }

    public void UpdateScore()
    {
        scoreText.text = GameManager.Instance.score.ToString();

        _scoreTween.Kill();
        _scoreTween = _scoreTransform.DOJumpAnchorPos(_initScorePos, scoreJumpHeight, 1, scoreJumpDuration).SetEase(Ease.InOutQuad);
    }

    public void ShowGameOverScreen()
    {
        StartCoroutine(ShowGameOverScreenRoutine());
    }

    IEnumerator ShowGameOverScreenRoutine()
    {
        gameOverPanel.gameObject.SetActive(true);
        finalScoreText.gameObject.SetActive(false);

        Image gameOverPanelImage = gameOverPanel.GetComponent<Image>();

        gameOverPanelImage.DOFade(0f, 0f);

        yield return null;

        gameOverPanelImage.DOFade(gameOverPanelFade, gameOverTransitionDuration);

        yield return new WaitForSeconds(gameOverTransitionDuration);

        finalScoreText.gameObject.SetActive(true);

        _finalScoreTween.Kill();
        _finalScoreTween = DOTween.To(() => finalScoreDisplayNumber, x => finalScoreDisplayNumber = x, GameManager.Instance.score, gameOverTransitionDuration * 2f).SetEase(Ease.OutExpo);
    }
}