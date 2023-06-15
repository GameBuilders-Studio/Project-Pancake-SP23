
using System;
using UnityEngine;

public class InGameProgress : MonoBehaviour
{
    [SerializeField] private Transform progressAnchor;
    [SerializeField] private GameObject progressBG;

    private void Start()
    {
        SetProgress(0f);
    }

    public void SetProgress(float pg)
    {
        // after all, why shouldn't I
        // why shouldn't I write the most inefficient code known to humankind
        if (pg <= 0.01f || pg >= 0.99f)
        {
            progressAnchor.gameObject.SetActive(false);
            progressBG.SetActive(false);
        }
        else
        {
            progressAnchor.gameObject.SetActive(true);
            progressBG.SetActive(true);
        }
        progressAnchor.localScale = new Vector3(
            pg,
            progressAnchor.localScale.y,
            progressAnchor.localScale.z
            );
    }
}
