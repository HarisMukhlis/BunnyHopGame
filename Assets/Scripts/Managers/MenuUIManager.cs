using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private RectTransform titlePanel;
    [SerializeField] private RectTransform buttonsPanel;
    [Space]
    [SerializeField] private TextMeshProUGUI titleText;
    [Space]
    [SerializeField] private List<MenuUIButton> buttonsList;
    [Space]
    [SerializeField] private Image blackscreenPanel;

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 3f;
    [Space]
    [SerializeField] private Vector2 titlePanelOffset;
    [SerializeField] private Vector2 titleTextOffset;
    [Space]
    [SerializeField] private Vector2 buttonsPanelOffset;
    [Space]
    [SerializeField] private float buttonsListStagger = .4f;

    [Header("Scene")]
    [SerializeField] private string gameScene = "GameplayScene";

    private Image _titlePanelImage;
    private bool _isTransitioning;
    // private Image _buttonsPanelImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void StartGame()
    {
        TransitionEnd();
    }

    void Start()
    {
        if (titlePanel != null)
            _titlePanelImage = titlePanel.gameObject.GetComponent<Image>();


        TransitionStart();
    }

    private void TransitionStart()
    {
        StartCoroutine(TransitionStartRoutine());
    }

    IEnumerator TransitionStartRoutine()
    {
        _isTransitioning = true;

        titlePanel.DOAnchorPos(titlePanel.anchoredPosition - titlePanelOffset, 0f);
        buttonsPanel.DOAnchorPos(buttonsPanel.anchoredPosition - buttonsPanelOffset, 0f);
        titleText.rectTransform.DOAnchorPos(titleText.rectTransform.anchoredPosition - titleTextOffset, 0f);

        float titlePanelInitFade = _titlePanelImage.color.a;
        _titlePanelImage.DOFade(0f, 0f);
        titleText.DOFade(0f, 0f);

        foreach (var button in buttonsList)
        {
            button.FadeOut(0f);
        }

        yield return new WaitForSeconds(transitionDuration / 3f);

        titleText.rectTransform.DOAnchorPos(titleText.rectTransform.anchoredPosition + titleTextOffset, transitionDuration / 3f).SetEase(Ease.OutExpo);
        titleText.DOFade(1f, transitionDuration / 3f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(transitionDuration / 3f);

        titlePanel.DOAnchorPos(titlePanel.anchoredPosition + titlePanelOffset, transitionDuration / 3f).SetEase(Ease.InOutExpo);
        _titlePanelImage.DOFade(titlePanelInitFade, transitionDuration / 3f);

        yield return new WaitForSeconds(transitionDuration / 3f);

        buttonsPanel.DOAnchorPos(buttonsPanel.anchoredPosition + buttonsPanelOffset, transitionDuration / 3f).SetEase(Ease.OutExpo);

        //stagger buttons
        foreach (var button in buttonsList)
        {
            button.FadeIn(transitionDuration / 3f);

            yield return new WaitForSeconds(buttonsListStagger);
        }

        _isTransitioning = false;
    }

    private void TransitionEnd()
    {
        StartCoroutine(TransitionEndRoutine());
    }

    IEnumerator TransitionEndRoutine()
    {
        if (!_isTransitioning)
        {
            _isTransitioning = true;

            blackscreenPanel.DOFade(1f, transitionDuration / 2f);

            yield return new WaitForSeconds(transitionDuration / 2f + .5f);

            StaticManager.Instance.LoadScene(gameScene);
        }
    }

    public void ExitGame()
    {
        if (!_isTransitioning)
        StaticManager.Instance.QuitApplication();
    }
}
