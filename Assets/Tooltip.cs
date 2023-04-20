using UnityEngine;
public class Tooltip : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 position = _camera.WorldToScreenPoint(_target.position + _offset);
        transform.position = position;
    }

}


