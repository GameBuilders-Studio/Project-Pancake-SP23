using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    private GameObject _dishPrefab;
    [SerializeField]
    private int _size = 3;
    [SerializeField]
    private int _count = 0;
    public int Count{
        get => _count;
        set => _count = value;
    }
    private void Awake() {
        _count = _size;
    }
    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            if(_count <= 0) return;
            var ingredientGo = Instantiate(_dishPrefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
            _count--;
        }
    }
}
