using UnityEngine;
using DG.Tweening;

public class PopupAnimation : MonoBehaviour
{
    public RectTransform panel;
    public float animationDuration = 0.5f;
    public float scale;
    void Start()
    {
        // Assume the panel is initially invisible (scaled down to zero)
        panel.localScale = Vector3.zero;
    }

    void OnEnable() {
        ShowPanel();
    }
    public void ShowPanel()
    {
        // Scale panel from 0 to 1
        panel.DOScale(scale, animationDuration).SetEase(Ease.OutBack);
    }

    public void HidePanel()
    {
        // Scale panel from 1 to 0
        panel.DOScale(0, animationDuration).SetEase(Ease.InBack);
    }
}