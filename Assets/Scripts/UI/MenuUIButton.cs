using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIButton : MonoBehaviour
{
    [SerializeField] private Image[] images;
    [SerializeField] private TextMeshProUGUI[] texts;

    public void FadeOut(float duration)
    {
        foreach (var image in images)
        {
            image.DOFade(0f, duration);
        }

        foreach (var text in texts)
        {
            text.DOFade(0f, duration);
        }
    }

    public void FadeIn(float duration)
    {
        foreach (var image in images)
        {
            image.DOFade(1f, duration);
        }

        foreach (var text in texts)
        {
            text.DOFade(1f, duration);
        }
    }
}