
using UnityEngine;
using UnityEngine.UI;

public class InGameProgress : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _border;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fill;
    [SerializeField] public GameObject WarningSign;
    [SerializeField] public GameObject Checkmark;

    [Header("Visuals Settings")]
    [SerializeField] private Vector3 _offset;


    private Transform _target;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        SetProgress(0f);
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }
        Vector3 position = _camera.WorldToScreenPoint(_target.position + _offset);
        transform.position = position;
    }

    public void SetProgress(float value)
    {
        // after all, why shouldn't I
        // why shouldn't I write the most inefficient code known to humankind
        if (value <= 0.01f || value >= 0.99f)
        {
            // Disable progress bar
            _border.gameObject.SetActive(false);
            _slider.gameObject.SetActive(false);
            _fill.gameObject.SetActive(false);
        }
        else
        {
            // Enable Progress Bar 
            _border.gameObject.SetActive(true);
            _slider.gameObject.SetActive(true);
            _fill.gameObject.SetActive(true);
        }
        _slider.value = value;
    }
}
