using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    private GameObject _dishPrefab;
    [SerializeField]
    private int _initPlates = 3;
    [SerializeField]
    private int _currentPlates = 0;
    public int CurrentPlates{
        get => _currentPlates;
        set => _currentPlates = value;
    }
    private void Awake() {
        _currentPlates = _initPlates;
    }
    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            if(_currentPlates <= 0) return;
            var ingredientGo = Instantiate(_dishPrefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
            _currentPlates--;
        }
    }
}
