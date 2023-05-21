using UnityEngine;
using DG.Tweening;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    public TMP_Text textObject;
    public float animationDuration = 1f;
    public Vector3 maxScale = new Vector3(1.1f, 1.1f, 1.1f);


    void Start()
    {
        AnimateText();
    }

    void AnimateText()
    {
        Vector3 originalScale = textObject.transform.localScale;

        // Create the scale animation
        Sequence scaleSequence = DOTween.Sequence();
        scaleSequence.Append(textObject.transform.DOScale(maxScale, animationDuration));
        scaleSequence.Append(textObject.transform.DOScale(originalScale, animationDuration));
        scaleSequence.SetLoops(-1);

        // Start the sequences
        scaleSequence.Play();

    }
}
