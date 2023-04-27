using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// UI elements in Overlay mode, will follow the target in world space
/// </summary>
public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Camera cam;
    [SerializeField] public float scale = 0.6f;               //scale of the UI element

    private void Awake()
    {
        cam = Camera.main;

#if UNITY_EDITOR
        Assert.IsNotNull(target);
        Assert.IsNotNull(cam);
#endif
    }

    private void LateUpdate()
    {
        Vector3 position = cam.WorldToScreenPoint(target.position + offset);

        if (transform.position != position)
        {
            transform.position = position;
        }

        transform.localScale = Vector3.one * scale;
    }

}

