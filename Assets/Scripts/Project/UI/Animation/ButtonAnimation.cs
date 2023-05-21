using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour
{
    public Button button;
    public Vector3 biggerScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float duration = 1f;

    private Tweener tweener;

    private void Start()
    {
        if (button == null)
        {
            Debug.LogError("Button reference is not set");
            return;
        }
        Vector3 originalScale = button.GetComponent<RectTransform>().localScale;
        Vector3 newScale = Vector3.Scale(originalScale, biggerScale);
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
        
        // Make sure the button starts at its original scale
        buttonRectTransform.localScale = originalScale;

        // Set up the tween
        tweener = buttonRectTransform.DOScale(newScale, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        // Always kill tweens when the object they're attached to is destroyed, 
        // or they will cause problems
        tweener.Kill();
    }
}
