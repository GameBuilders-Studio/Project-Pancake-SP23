using UnityEngine;
using UnityEngine.UI;
using CustomAttributes;
public class Tooltip : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField, Required] private GameObject _itemsPanel;


    private void Awake()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 position = _camera.WorldToScreenPoint(_target.position + _offset);
        transform.position = position;
    }

    public void AddIngredient(IngredientSO ingredient)
    {
        GameObject spriteObject = new();
        Image spriteImage = spriteObject.AddComponent<Image>();
        spriteImage.sprite = ingredient.icon;

        spriteObject.transform.SetParent(_itemsPanel.transform, false);
    }

    public void ClearIngredients()
    {
        foreach (Transform child in _itemsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

}


