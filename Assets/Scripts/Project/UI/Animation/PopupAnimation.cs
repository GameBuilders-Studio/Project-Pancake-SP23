using UnityEngine;
using DG.Tweening;

public class PopupAnimation : MonoBehaviour
{
    [SerializeField]
    private RectTransform panel;
    [SerializeField]
    private float animationDuration = 0.5f;
    [SerializeField]
    private float scale;
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