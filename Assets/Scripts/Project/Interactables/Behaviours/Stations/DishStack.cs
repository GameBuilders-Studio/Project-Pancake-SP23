using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    private GameObject _dishPrefab;
    [SerializeField]
    private int _maxPlates = 3;
    [SerializeField]
    private int _currentPlates = 0;
    private void Awake() {
        _currentPlates = _maxPlates;
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
