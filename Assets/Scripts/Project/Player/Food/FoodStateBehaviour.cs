using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodStateBehaviour : MonoBehaviour
{
    [SerializeField]
    private FoodStateData _foodState;

    [SerializeField]
    private Renderer _renderer;

    private float _progress;

    public FoodStateData FoodState
    {
        get => _foodState;
        set => _foodState = value;
    }

    void Awake()
    {
        if (_renderer == null) 
        { 
            _renderer = GetComponent<Renderer>(); 
        }
        _progress = 0.0f;
    }

    public void Show()
    {
        _renderer.enabled = true;
    }

    public void HideAndReset()
    {
        _progress = 0.0f;
    }

    public virtual void OnProgressUpdate(float progress)
    {
        _progress = progress;
        // handle specific animation behaviour here!
    }
}
