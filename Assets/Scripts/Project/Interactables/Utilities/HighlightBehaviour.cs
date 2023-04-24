using CustomAttributes;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HighlightBehaviour : MonoBehaviour
{
    [SerializeField, Required]
    private Renderer _renderer;

    protected Material _material;

    private int _highlightPropertyId;

    void Awake()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
        _material = _renderer.material;
        _highlightPropertyId = Shader.PropertyToID("_Use_Highlight_Shader");
        _renderer.material.EnableKeyword("_Use_Highlight_Shader");
    }

    public void SetHighlight(bool enabled = true)
    {
        if (enabled)
        {
            _renderer.material.SetFloat(_highlightPropertyId, 1);
        }
        else
        {
            _renderer.material.SetFloat(_highlightPropertyId, 0);
        }
    }
}
